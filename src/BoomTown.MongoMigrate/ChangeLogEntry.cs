using System;
using MongoDB.Bson;

namespace BoomTown.MongoMigrate
{
    internal class ChangeLogEntry
    {
        /// <summary>
        /// The Mongo Generated Id
        /// </summary>
        public ObjectId Id { get; set; }
        /// <summary>
        /// The Date the Migration was applied
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// The Name of the Migration that was used
        /// </summary>
        public string Name { get; set; }
    }
}