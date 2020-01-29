using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BoomTown.MongoMigrate
{
    /// <summary>
    /// Class to manage Mongo Migrations
    /// </summary>
    /// <typeparam name="T">A Class in the same assembly as the <see cref="IMigration"/> files</typeparam>
    public class MongoMigrationsRunner<T> where T : class
    {
        private const string ChangeLogCollectionName = "changelog";
        private readonly IMongoCollection<ChangeLogEntry> _migrationCollection;
        private readonly IMongoDatabase _database;
        
        /// <summary>
        /// A Migrations Runner 
        /// </summary>
        /// <param name="database">The database to run the migrations</param>
        public MongoMigrationsRunner(IMongoDatabase database)
        {
            _database = database;
            _migrationCollection = _database.GetCollection<ChangeLogEntry>(ChangeLogCollectionName);
        }
        
        /// <summary>
        /// Run all unapplied Migrations
        /// </summary>
        /// <returns>A list of MigrationResults that were applied during the run</returns>
        public async Task<List<MigrationResult>> Up()
        {
            var results = new List<MigrationResult>();
            var changeLogs = typeof(T).Assembly.GetTypes()
                .Where(x => typeof(IMigration).IsAssignableFrom(x) && x.IsClass)
                .Select(y => (IMigration)Activator.CreateInstance(y));

            foreach (var changeLog in changeLogs.OrderBy(x => x.ChangeDate()))
            {
                var changeLogName = changeLog.GetType().Name;
                var exists = await _migrationCollection.AsQueryable().AnyAsync(x => x.Name == changeLogName);

                if (exists)
                    continue;

                var migrationResult = new ChangeLogEntry {DateCreated = DateTime.UtcNow, Name = changeLogName};

                await changeLog.Up(_database);
                await _migrationCollection.InsertOneAsync(migrationResult);
                results.Add(new MigrationResult(changeLogName, migrationResult.DateCreated));
            }

            return results;
        }

        /// <summary>
        /// Undo the last Migration. <br/>
        /// This will delete it from the changelog so it can be reapplied later. <br/>
        /// If no migrations are left, this method will return null
        /// </summary>
        /// <returns>The Migration that was removed</returns>
        /// <exception cref="ArgumentException">If a Migration class cannot be found</exception>
        public async Task<MigrationResult> Down()
        {
            var latestChangeLog = await _migrationCollection.AsQueryable()
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefaultAsync();

            if (latestChangeLog == null)
            {
                return null;
            }
            
            var changeLogClass = typeof(T).Assembly.GetTypes().FirstOrDefault(x => x.Name == latestChangeLog.Name);

            if (changeLogClass == null)
                throw new ArgumentException("Failed to find class with matching name. Make sure no IMigration classes have been removed.");

            var changeLog = (IMigration)Activator.CreateInstance(changeLogClass);
            
            await changeLog.Down(_database);
            await _migrationCollection.DeleteOneAsync(x => x.Name == latestChangeLog.Name);
            return new MigrationResult(latestChangeLog.Name, latestChangeLog.DateCreated);
        }
        
        /// <summary>
        /// Undo all the migrations, returning the database to its original fix  
        /// </summary>
        /// <returns>A list of the undid migrations</returns>
        public async Task DownAll()
        {
            MigrationResult result;

            do
            {
                result = await Down();
            } while (result != null);
        }

        /// <summary>
        /// Get a list of all the applied Migrations
        /// </summary>
        /// <returns>List of Migrations</returns>
        public async Task<List<MigrationResult>> GetAppliedMigrations()
        {
            return await _migrationCollection.AsQueryable().Select(x => 
                new MigrationResult(x.Name, x.DateCreated)).ToListAsync();
        }
    }
}