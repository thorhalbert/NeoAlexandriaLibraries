using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NeoCommon.MongoModels
{
    public static class DiskWatcher
    {
        static IMongoCollection<BsonDocument> _diskWatcher;

        static  DiskWatcher()
        {
            _diskWatcher = NeoMongo.NeoDb.GetCollection<BsonDocument>("DiskWatcher");
        }

        public static async Task<List<BsonDocument>> Get()
        {
            var docs = await _diskWatcher.Find<BsonDocument>(_ => true).ToListAsync();

            return docs;
        }
    }
}
