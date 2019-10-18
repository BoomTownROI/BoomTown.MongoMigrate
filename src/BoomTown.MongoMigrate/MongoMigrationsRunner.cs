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
    /// <typeparam name="T">A Class in the same assembly as the <see cref="IChangeSet"/> files</typeparam>
    public class MongoMigrationsRunner<T> where T : class
    {
        private const string ChangeLogCollectionName = "changelog";
        private readonly IMongoCollection<ChangeLogEntry> _changeSetCollection;
        private readonly IMongoDatabase _database;
        
        /// <summary>
        /// A Migrations Runner 
        /// </summary>
        /// <param name="database">The database to run the migrations</param>
        public MongoMigrationsRunner(IMongoDatabase database)
        {
            _database = database;
            _changeSetCollection = _database.GetCollection<ChangeLogEntry>(ChangeLogCollectionName);
        }
        
        /// <summary>
        /// Run all unapplied Migrations
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task Up()
        {
            var changeLogs = typeof(T).Assembly.GetTypes()
                .Where(x => typeof(IChangeSet).IsAssignableFrom(x) && x.IsClass)
                .Select(y => (IChangeSet)Activator.CreateInstance(y));
            
            foreach (var changeLog in changeLogs.OrderBy(x => x.ChangeDate()))
            {
                var changeLogName = changeLog.GetType().Name;
                var exists = await _changeSetCollection.AsQueryable().AnyAsync(x => x.Name == changeLogName);

                if (exists) 
                    continue;
                    
                var changeSetResult = new ChangeLogEntry{DateCreated = DateTime.UtcNow, Name = changeLogName};
                
                try
                {
                    await changeLog.Up(_database);
                    await _changeSetCollection.InsertOneAsync(changeSetResult);
                    Console.WriteLine($"Applied {changeLogName}");
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed applying {changeLogName}");
                    throw;
                }
            }
            
            Console.WriteLine("All Change Sets successfully applied");
        }

        /// <summary>
        /// Undo the last ran ChangeSet.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Down()
        {
            var latestChangeLog = await _changeSetCollection.AsQueryable()
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefaultAsync();

            if (latestChangeLog == null)
            {
                Console.WriteLine("No Change sets left to revert");
                return;
            }
            
            var changeLogClass = typeof(T).Assembly.GetTypes().FirstOrDefault(x => x.Name == latestChangeLog.Name);

            if (changeLogClass == null)
                throw new ArgumentException("Failed to find class with matching name. Make sure no ChangeSets have been removed.");

            var changeLog = (IChangeSet)Activator.CreateInstance(changeLogClass);

            try
            {
                await changeLog.Down(_database);
                await _changeSetCollection.DeleteOneAsync(x => x.Name == latestChangeLog.Name);
                Console.WriteLine($"Reverted {latestChangeLog.Name}");
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to Revert {latestChangeLog.Name}");
                throw;
            }
        }

        public async Task<List<ChangeLogEntry>> GetAppliedChangeSets()
        {
            return await _changeSetCollection.AsQueryable().ToListAsync();
        }
    }
}