using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoBakedVolumes.Mongo
{

    public class BakedPools
    {
        public string _id { get; set; }
        [BsonRequired] public BsonDocument Affinity { get; set; }   // 60/60
        [BsonRequired] public string Filesystem { get; set; }   // 60/60
        [BsonRequired] public BakedPools_Mounts Mount { get; set; }   // 60/60
        [BsonRequired] public string Name { get; set; } // 60/60
        [BsonRequired] public string Path { get; set; } // 60/60
        [BsonRequired] public string Server { get; set; }   // 60/60
        [BsonRequired] public string State { get; set; }    // 60/60
        [BsonRequired] public string Status { get; set; }   // 60/60
        [BsonRequired] public double SubDirs { get; set; } // 60/60
        [BsonRequired] public double Trust { get; set; }   // 60/60
        [BsonRequired] public DateTime Updated { get; set; }   // 60/60
        [BsonIgnoreIfNull] public double? ComfortLevel { get; set; }    // 46/60
        [BsonIgnoreIfNull] public double? MinimumFree { get; set; } // 46/60
        [BsonIgnoreIfNull] public bool? Closed { get; set; }    // 28/60
        [BsonIgnoreIfNull] public bool? ReadOnly { get; set; }  // 18/60
        [BsonIgnoreIfNull] public bool? Evacuate { get; set; }  // 14/60
        [BsonIgnoreIfNull] public bool? Dead { get; set; }  // 4/60
        [BsonIgnoreIfNull] public double? MininumFree { get; set; } // 2/60
        [BsonIgnoreIfNull] public bool? Offline { get; set; }   // 2/60
        [BsonIgnoreIfNull] public bool? ZPool { get; set; } // 2/60

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }
    public class BakedPools_Mounts
    {
        [BsonRequired] public string Mount { get; set; }    // 60/60
        [BsonRequired] public string Name { get; set; } // 60/60
        [BsonRequired] public string Options { get; set; }  // 60/60
        [BsonRequired] public string Type { get; set; } // 60/60
    }
}
