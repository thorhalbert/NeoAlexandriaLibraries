using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

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
            var setti = MongoClientSettings.FromConnectionString(conn);
            setti.ConnectTimeout = new TimeSpan(0, 5, 0);
            setti.ServerSelectionTimeout = new TimeSpan(0, 5, 0);
            setti.SocketTimeout = new TimeSpan(0, 5, 0);

            Connection = new MongoClient(setti);

            NeoDb = Connection.GetDatabase("NeoAlexandria");
        }

    }
}
