using MongoDB.Bson;
using MongoDB.Driver;
using System;
using NeoAssets.Mongo;
using NeoBakedVolumes.Mongo;
using Grpc.Net.Client;
using BakedFileService.Protos;
using System.Security.Cryptography;
using System.Linq;

namespace Tester
{
    class Program
    {
        static int  Main(string[] args)
        {
            var conn = Environment.GetEnvironmentVariable("MONGO_URI");
            var Connection = new MongoClient(conn);

            var NeoDb = Connection.GetDatabase("NeoAlexandria");

            var asset = NeoDb.GetCollection<BakedVolumes>("BakedVolumes");
            var bac = NeoDb.GetCollection<BakedAssets>("BakedAssets");

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

            //var channel = GrpcChannel.ForAddress("http://feanor:5000");
            //var assetClient = new BakedVolumeData.BakedVolumeDataClient(channel);

            //var st = DateTime.Now;

            //var ret = assetClient.Fetch(new FetchRequest
            //{
            //    BakedVolume = "NEO_ASSETS_00001",
            //    Part = "A",
            //    Offset = 0,
            //    RequestCount = 1024 * 1024
            //});

            //var tm = DateTime.Now - st;

            //var startHash = "0f49f6c69d7cecf96ec362dac94745c6425c98ee"; // Hello world achieved 2021/02/09
            var startHash = "4146c29c78049b34c8b4196eb406743ce5f6eeec"; // Should be split across volumes (and is very big)- success

             var f = new AssetFileSystem.AssetFile(startHash, NeoDb, bac);
            
            var str = f.CreateReadStream();

            // Tap the stream so we can get a hash on it.
            //var hashStr = new NeoCommon.HashStream(str, HashAlgorithm.Create("SHA1"));

            var buf = new Byte[32*1024*1024];

            while (true)
            {
                var len = str.Read(buf, 0, buf.Length);
                if (len < 1)
                    break;
            }

            //var hash = string.Concat(hashStr.Hash().Select(x => x.ToString("x2")));
            //if (startHash != hash)
            //    Console.WriteLine("Hashes don't match");

            return 0;
        }
    }
}
