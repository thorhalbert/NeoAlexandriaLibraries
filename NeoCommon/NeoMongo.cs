using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NeoCommon
{
    public static class NeoMongo
    {
        public static MongoClient Connection { get; private set; }
        public static IMongoDatabase NeoDb { get; private set; }

        static NeoMongo()
        {
            if (NeoDb != null)
                return;  // Already good, though this shouldn't happen

            var conn = Environment.GetEnvironmentVariable("MONGO_URI");
            Connection = new MongoClient(conn);

            NeoDb = Connection.GetDatabase("NeoAlexandria");
        }

    }
}
