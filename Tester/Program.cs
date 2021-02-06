using MongoDB.Bson;
using MongoDB.Driver;
using System;
using NeoAssets.Mongo;
using NeoBakedVolumes.Mongo;
using Grpc.Net.Client;
using BakedFileService.Protos;

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

            //using (var cursor = await asset.FindAsync(filter))
            //{
            //    while (await cursor.MoveNextAsync())
            //    {
            //        var batch = cursor.Current;
            //        foreach (var b in batch)
            //            Console.WriteLine(b._id);
            //    }
            // }

            // Call Baked Asset server and get a record

            var channel = GrpcChannel.ForAddress("http://feanor:5000");
            var assetClient = new BakedVolumeData.BakedVolumeDataClient(channel);

            var st = DateTime.Now;

            var ret = assetClient.Fetch(new FetchRequest
            {
                BakedVolume = "NEO_ASSETS_00001",
                Part = "A",
                Offset = 0,
                RequestCount = 1024 * 1024
            });

            var tm = DateTime.Now - st;


        }
    }
}
