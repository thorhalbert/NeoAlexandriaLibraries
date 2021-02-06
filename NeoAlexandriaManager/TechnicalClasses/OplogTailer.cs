using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAlexandriaManager.TechnicalClasses
{
    public class OplogTailer
    {
        // Moving this out of common since we need .net standard 2.1 + to make the async stuff work

        
        public string Collection { get; private set; }
        private MongoClient Connection;
        private IMongoDatabase NeoDb;
        private IMongoCollection<BsonDocument> opLog;
        private BsonTimestamp now;
        private bool up;

        public OplogTailer(string collection)
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

            up = true;
        }
        public void Stop()
        {
            // Just let it garbage collect for now
            up = false;
        }

        // Can't really do IAsyncEnumerable until we can do standard 2.1+
        public async IAsyncEnumerable<BsonDocument> ProcessEnumeration()
        {
            var query = Query.And(
                Query.GTE("ts", now),
                Query.EQ("ns", Collection));

            var options = new FindOptions<BsonDocument>
            {
                CursorType = CursorType.TailableAwait,
                NoCursorTimeout = true,
                Sort = Builders<BsonDocument>.Sort.Ascending("$natural")
            };

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Gte(n => n["ts"], now) & builder.Eq(n => n["ns"], Collection);

            while (up)
            {
                using (var cursor = await opLog.FindAsync<BsonDocument>(filter, options))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var d in batch)
                            yield return d;
                    }
                }
            }
        }
    }
}

