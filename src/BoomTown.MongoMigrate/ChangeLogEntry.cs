using System;
using MongoDB.Bson;

namespace BoomTown.MongoMigrate
{
    public class ChangeLogEntry
    {
        /// <summary>
        /// The Mongo Generated Id
        /// </summary>
        public ObjectId Id { get; set; }
        /// <summary>
        /// The Date the ChangeSet was applied
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// The Name of the ChangeSet that was used
        /// </summary>
        public string Name { get; set; }
    }
}