using MongoDB.Bson;
using MongoDB.Driver;
using System;
using NeoAssets.Mongo;
using NeoBakedVolumes.Mongo;

namespace Tester
{
    class Progra
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var conn = Environment.GetEnvironmentVariable("MONGO_URI");
            var Connection = new MongoClient(conn);

            var NeoDb = Connection.GetDatabase("NeoAlexandria");

            var asset = NeoDb.GetCollection<BakedVolumes>("BakedVolumes");

            var filter = Builders<BakedVolumes>.Filter.Empty;

            using (var cursor = await asset.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var b in batch)
                        Console.WriteLine(b._id);
                }
            }
        }
    }
}
