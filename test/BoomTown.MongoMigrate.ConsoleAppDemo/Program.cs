using System;
using System.Linq;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Driver;

namespace BoomTown.MongoMigrate.ConsoleAppDemo
{
    public class Program
    {
        static async Task Main()
        {
            // Create an IMongoDatabase, in this case it will just be an in-memory test one.
            var mongoToGo = MongoDbRunner.Start();
            var mongoClient = new MongoClient(mongoToGo.ConnectionString);
            var database = mongoClient.GetDatabase("NewDatabase");

            var runner = new MongoMigrationsRunner<SampleMigration>(database);

            // Run the migrations
            var results = await runner.Up();
            
            results.Select(x => $"Name - {x.Name}, Date Created - {x.DateCreated}").ToList()
                .ForEach(Console.WriteLine);
            
            // Undo the migrations
            await runner.DownAll();
            
            mongoToGo.Dispose();
        }
    }
}