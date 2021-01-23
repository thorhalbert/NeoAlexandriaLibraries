using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeoAlexandriaManager.Data
{
    public class DiskWatcherService
    {
        public Task<List<BsonDocument>> GetDisks()
        {
            return NeoCommon.MongoModels.DiskWatcher.Get();
        }


    }
}
