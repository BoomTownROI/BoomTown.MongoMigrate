using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace BoomTown.MongoMigrate.ConsoleAppDemo
{
    public class AnotherMigration : IChangeSet
    {
        public DateTime ChangeDate()
        {
            return new DateTime(2019, 10, 19);
        }

        public async Task Up(IMongoDatabase database)
        {
            await database.CreateCollectionAsync("AnotherCollection");
        }

        public async Task Down(IMongoDatabase database)
        {
            await database.DropCollectionAsync("AnotherCollection");
        }
    }
}