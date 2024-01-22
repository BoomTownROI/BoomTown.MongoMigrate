using System;
using System.Linq;
using System.Threading.Tasks;
using EphemeralMongo;
using MongoDB.Driver;
using Xunit;

namespace BoomTown.MongoMigrate.Test
{
    public class MongoMigrationsRunnerTests : IDisposable
    {
        private readonly IMongoRunner _mongo;
        private readonly MongoMigrationsRunner<SampleMigration> _runner;
        private readonly IMongoDatabase _database;
        
        public MongoMigrationsRunnerTests()
        {
            _mongo = MongoRunner.Run();
            var client = new MongoClient(_mongo.ConnectionString);
            _database = client.GetDatabase("mongo");

            _runner = new MongoMigrationsRunner<SampleMigration>(_database);
        }
        
        public void Dispose()
        {
            _mongo?.Dispose();
            /*if (!_mongo.Disposed)
                _mongo.Dispose();*/
        }
        
        [Fact]
        public async Task CanMigrateUpAndDown()
        {
            var result = await _runner.Up();

            var collections = (await _database.ListCollectionsAsync()).ToList();
            
            // Only the ChangeLog collection should be created
            Assert.Single(collections);

            // Assert both change logs ran. 
            Assert.Equal(2, result.Count);
            
            // Run it again to make sure changes are only applied once
            var secondResult = await _runner.Up();
            Assert.Empty(secondResult);
            
            // Ensure order was applied correctly
            var changeLogs = await _runner.GetAppliedMigrations();
            Assert.Equal("SampleMigration", changeLogs.First().Name);
            Assert.Equal("SecondaryMigration", changeLogs.Last().Name);
            
            var firstRevert = await _runner.Down();
            var secondRevert = await _runner.Down();
            var thirdRevert = await _runner.Down();

            Assert.Equal("SecondaryMigration", firstRevert.Name);
            Assert.Equal("SampleMigration", secondRevert.Name);
            Assert.Null(thirdRevert);
        }
        
        // ReSharper disable once ClassNeverInstantiated.Local
        private class SampleMigration : IMigration
        {
            public DateTime ChangeDate() { return DateTime.Today; }

            public Task Up(IMongoDatabase database) { return Task.CompletedTask; }

            public Task Down(IMongoDatabase database) { return Task.CompletedTask; }
        }
        
        // ReSharper disable once UnusedMember.Local
        private class SecondaryMigration : IMigration
        {
            public DateTime ChangeDate() { return DateTime.Today.AddDays(1); }

            public Task Up(IMongoDatabase database) { return Task.CompletedTask; }

            public Task Down(IMongoDatabase database) { return Task.CompletedTask; }
        }
    }
}