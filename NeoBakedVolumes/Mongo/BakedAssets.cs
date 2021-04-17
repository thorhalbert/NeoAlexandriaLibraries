using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoBakedVolumes.Mongo
{

    public static class BakedAssets_exts
    {
        public static IMongoCollection<BakedAssets> BakedAssets(this IMongoDatabase db)
        {
            return db.GetCollection<BakedAssets>("BakedAssets");
        }
    }
    public class BakedAssets
    {
        public string _id { get; set; }
        [BsonIgnoreIfNull] public bool? Annealed { get; set; }  // 400000/400000
        [BsonRequired] public UInt32 Block { get; set; }   // 400000/400000
        [BsonRequired] public string Comp { get; set; } // 400000/400000
        [BsonRequired] public UInt64 FileLength { get; set; }  // 400000/400000
        [BsonRequired] public UInt32 Offset { get; set; }  // 400000/400000
        [BsonRequired] public string Part { get; set; } // 400000/400000
        [BsonRequired] public UInt64 RealLength { get; set; }  // 400000/400000
        [BsonRequired] public string Volume { get; set; }   // 400000/400000
        [BsonIgnoreIfNull] public double? CTime { get; set; }   // 338138/400000
        [BsonIgnoreIfNull] public List<string> Owners { get; set; } // 8/400000

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }
}
