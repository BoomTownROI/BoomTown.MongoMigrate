using System;

namespace BoomTown.MongoMigrate
{
    /// <summary>
    /// A class to hold the results for the methods
    /// </summary>
    public class MigrationResult
    {
        public MigrationResult(string name, DateTime dateCreated)
        {
            Name = name;
            DateCreated = dateCreated;
        }
        
        /// <summary>
        /// The Name of the Migration
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The Date the Migration was applied
        /// </summary>
        public DateTime DateCreated { get; }
    }
}