using System;
using System.Linq;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace BoomTown.MongoMigrate.Test
{
    public class MongoMigrationsRunnerTests : IDisposable
    {
        private readonly MongoDbRunner _mongo;
        private readonly MongoMigrationsRunner<SampleMigration> _runner;
        private readonly IMongoDatabase _database;
        
        public MongoMigrationsRunnerTests()
        {
            _mongo = MongoDbRunner.Start();
            var client = new MongoClient(_mongo.ConnectionString);
            _database = client.GetDatabase("mongo");

            _runner = new MongoMigrationsRunner<SampleMigration>(_database);
        }
        
        public void Dispose()
        {
            if (!_mongo.Disposed)
                _mongo.Dispose();
        }
        
        [Fact]
        public async Task CanMigrateUpAndDown()
        {
            await _runner.Up();

            var collections = (await _database.ListCollectionsAsync()).ToList();
            var changeLogs = await _runner.GetAppliedChangeSets();
            
            // Only the ChangeLog collection should be created
            Assert.Single(collections);

            // Assert both change logs ran. 
            Assert.Equal(2, changeLogs.Count);
            
            Assert.Equal("SampleMigration", changeLogs.First().Name);
            Assert.Equal("SecondaryMigration", changeLogs.Last().Name);

            for (var i = 0; i < 2; i++)
            {
                await _runner.Down();
            }
        }
        
        // ReSharper disable once ClassNeverInstantiated.Local
        private class SampleMigration : IChangeSet
        {
            public DateTime ChangeDate() { return DateTime.Today; }

            public Task Up(IMongoDatabase database) { return Task.CompletedTask; }

            public Task Down(IMongoDatabase database) { return Task.CompletedTask; }
        }
        
        // ReSharper disable once UnusedMember.Local
        private class SecondaryMigration : IChangeSet
        {
            public DateTime ChangeDate() { return DateTime.Today.AddDays(1); }

            public Task Up(IMongoDatabase database) { return Task.CompletedTask; }

            public Task Down(IMongoDatabase database) { return Task.CompletedTask; }
        }
    }
}