using MongoDB.Driver;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;

public class StorageStatus : Hub
{
    private MongoClient _mongoClient;
    private IAsyncEnumerable<BsonDocument> _oplogStream;

    public StorageStatus()
    {
        // Connect to MongoDB replica set
        _mongoClient = new MongoClient("mongodb://localhost:27017/?replicaSet=rs0");  // Replace with your connection string
    }

    private async IAsyncEnumerable<BsonDocument> GetStorageUpdates()
    {

        var database = _mongoClient.GetDatabase("local");
        var oplogCollection = database.GetCollection<BsonDocument>("oplog.rs");

        // Filter for the desired collection
        var filter = new BsonDocument("ns", new BsonDocument("$regex", "^myDatabase\\.myCollection\\.\\$"));
        var options = new FindOptions<BsonDocument> { CursorType = CursorType.TailableAwait };

        using (var cursor = await oplogCollection.FindAsync(filter, options))
        {
            while (await cursor.MoveNextAsync())
            {
                var oplogEntry = cursor.Current;
                foreach (var opEntry in oplogEntry)
                    if (oplogEntry is not null) yield return opEntry;
            }
        }

    }
}
