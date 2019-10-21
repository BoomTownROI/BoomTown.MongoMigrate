# BoomTown.MongoMigrate
A .Net library to help manage Mongo migrations. Similar to [migrate-mongo](https://github.com/seppevs/migrate-mongo) but with .Net

## Getting Started 

Install BoomTown.MongoMigrate

Create a IChangeSet
```c#
private class SampleMigration : IChangeSet
{
    public DateTime ChangeDate() { //Todays Date }

    public Task Up(IMongoDatabase database) { // Your Migration }

    public Task Down(IMongoDatabase database) { // How to undo your migration }
}
```

Run Up()
```c#
var database = new MongoClient(connectionString).GetDatabase("users");
var runner = new MongoMigrationsRunner<SampleMigration>(database);

// Run the migrations
await runner.Up();
```

Check the Status of the migrations
```c#
await _runner.GetAppliedChangeSets();
```

If you need to undo your last change
```c#
await _runner.Down()
```
