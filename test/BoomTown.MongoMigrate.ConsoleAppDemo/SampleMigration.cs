using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace BoomTown.MongoMigrate.ConsoleAppDemo
{
    public class SampleMigration : IMigration
    {
        public DateTime ChangeDate()
        {
            return new DateTime(2019, 10, 18);
        }

        public async Task Up(IMongoDatabase database)
        {
            await database.CreateCollectionAsync("Sample");
        }

        public async Task Down(IMongoDatabase database)
        {
            await database.DropCollectionAsync("Sample");
        }
    }
}