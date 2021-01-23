using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoRepositories.Mongo
{
    public class AssetDirs
    {
        public string _id { get; set; }
        [BsonRequired] public bool Deleted { get; set; }   // 2000000/2000000
        [BsonRequired] public string Parent { get; set; }   // 2000000/2000000
        [BsonRequired] public string Path { get; set; } // 2000000/2000000
        [BsonRequired] public string Repo { get; set; } // 2000000/2000000
        [BsonRequired] public string RepoType { get; set; } // 2000000/2000000
        [BsonRequired] public UInt32 Version { get; set; } // 2000000/2000000
        [BsonRequired] public string StateUUID { get; set; }    // 1999584/2000000
        [BsonRequired] public double LastSeen { get; set; }    // 1997360/2000000
        [BsonRequired] public Byte[] PathPhys { get; set; } // 1997360/2000000
        [BsonRequired] public AssetDirs_Stats Stat { get; set; } // 1997360/2000000

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }
    public class AssetDirs_Stats
    {
        [BsonRequired] public double atime { get; set; }   // 1997360/2000000
        [BsonRequired] public double ctime { get; set; }   // 1997360/2000000
        [BsonRequired] public UInt32 gid { get; set; } // 1997360/2000000
        [BsonRequired] public UInt32 mode { get; set; }    // 1997360/2000000
        [BsonRequired] public double mtime { get; set; }   // 1997360/2000000
        [BsonRequired] public UInt32 size { get; set; }    // 1997360/2000000
        [BsonRequired] public UInt32 uid { get; set; } // 1997360/2000000
    }


}
