using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoRepositories.Mongo
{
    public static class AssetFiles_exts
    {
        public static IMongoCollection<AssetFiles> AssetFiles(this IMongoDatabase db)
        {
            return db.GetCollection<AssetFiles>("AssetFiles");
        }
    }
    public class AssetFiles
    {
        public string _id { get; set; }
        [BsonRequired] public string Path { get; set; } // 2000000/2000000
        [BsonRequired] public string SHA1 { get; set; } // 2000000/2000000
        [BsonRequired] public bool Deleted { get; set; }   // 1949584/2000000
        [BsonRequired] public UInt32 Version { get; set; } // 1939688/2000000
        [BsonRequired] public BsonDocument Tenants { get; set; } // 1937224/2000000
        [BsonRequired] public bool Annealed { get; set; }  // 1936260/2000000
        [BsonRequired] public double LastSeen { get; set; }    // 1936260/2000000
        [BsonRequired] public string Parent { get; set; }   // 1936260/2000000
        [BsonRequired] public Byte[] PathPhys { get; set; }    // 1936260/2000000
        [BsonRequired] public string Repo { get; set; } // 1936260/2000000
        [BsonRequired] public string RepoType { get; set; } // 1936260/2000000
        [BsonRequired] public AssetFiles_Stat Stat { get; set; }    // 1936260/2000000
        [BsonRequired] public string StateUUID { get; set; }    // 1936260/2000000
        [BsonIgnoreIfNull] public string DirId { get; set; }    // 1868874/2000000
        [BsonIgnoreIfNull] public string NARP { get; set; } // 1862818/2000000
        //[BsonIgnoreIfNull] public BsonDocument Owners { get; set; } // 763544/2000000
        [BsonIgnoreIfNull] public string LastRealPath { get; set; } // 147060/2000000
        [BsonIgnoreIfNull] public BsonDocument History { get; set; }   // 10258/2000000

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }

    public class AssetFiles_Stat
    {
       
        [BsonRequired] public Int64 atime { get; set; }   // 1936260/2000000
        [BsonRequired] public Int64 ctime { get; set; }   // 1936260/2000000
        [BsonRequired] public UInt32 gid { get; set; } // 1936260/2000000
        [BsonRequired] public UInt32 mode { get; set; }    // 1936260/2000000
        [BsonRequired] public Int64 mtime { get; set; }   // 1936260/2000000
        [BsonRequired] public UInt32 size { get; set; }    // 1936260/2000000
        [BsonRequired] public UInt32 uid { get; set; } // 1936260/2000000
    }


}
