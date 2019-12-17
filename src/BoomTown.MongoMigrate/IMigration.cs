using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace BoomTown.MongoMigrate
{
    /// <summary>
    /// The changes that are going to be executed on a Mongo Database. <br/>
    /// This class must have a default constructor
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Date the Migration was created. The order of the Migrations are based on this value. 
        /// </summary>
        /// <returns>A DateTime</returns>
        DateTime ChangeDate();
        /// <summary>
        /// The Mongo Update to run
        /// </summary>
        /// <param name="database">The Mongo Database for the Migration</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task Up(IMongoDatabase database);
        /// <summary>
        /// The Inverse of the Mongo Update, this should reset the Database to its original state
        /// </summary>
        /// <param name="database">The Mongo Database for the Migration</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task Down(IMongoDatabase database);
    }
}