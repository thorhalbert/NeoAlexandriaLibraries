using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCommon
{
    public class NeoMongoOplogTailer
    {
        public string Collection { get; private set; }
        private MongoClient Connection;
        private IMongoDatabase NeoDb;
        private IMongoCollection<BsonDocument> opLog;
        private BsonTimestamp now;

        public NeoMongoOplogTailer(string collection)
        {
            Collection = collection;
        }

        public void Start()
        {
            var conn = Environment.GetEnvironmentVariable("MONGO_OPLOG_URI");
            Connection = new MongoClient(conn);

            NeoDb = Connection.GetDatabase("local");

            opLog = NeoDb.GetCollection<BsonDocument>("oplog.rs");

            now = (BsonTimestamp)DateTime.Now;
        }
        public void Stop()
        {
            // Just let it garbage collect for now
        }

        // Can't really do IAsyncEnumerable until we can do standard 2.1+
       
    }
}
