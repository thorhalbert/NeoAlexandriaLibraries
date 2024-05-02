using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace NeoCommon.MongoModels
{
    public static class DiskWatcherBase
    {
        static IMongoCollection<BsonDocument> _diskWatcher;

        static  DiskWatcherBase()
        {
            _diskWatcher = NeoMongo.NeoDb.GetCollection<BsonDocument>("DiskWatcher");
        }

        public static async Task<List<BsonDocument>> Get()
        {
            var docs = await _diskWatcher.Find<BsonDocument>(_ => true).ToListAsync();

            return docs;
        }


    }

  
    public class DiskWatcher_OptionInfo
    {
        [BsonIgnoreIfNull] public bool? rw { get; set; }        // 94/106
        [BsonIgnoreIfNull] public bool? noatime { get; set; }   // 92/106
        [BsonIgnoreIfNull] public string addr { get; set; }     // 68/106
        [BsonIgnoreIfNull] public string local_lock { get; set; }       // 68/106
        [BsonIgnoreIfNull] public UInt32? namlen { get; set; }  // 68/106
        [BsonIgnoreIfNull] public string proto { get; set; }    // 68/106
        [BsonIgnoreIfNull] public UInt32? retrans { get; set; } // 68/106
        [BsonIgnoreIfNull] public UInt32? rsize { get; set; }   // 68/106
        [BsonIgnoreIfNull] public string sec { get; set; }      // 68/106
        [BsonIgnoreIfNull] public bool? soft { get; set; }      // 68/106
        [BsonIgnoreIfNull] public UInt32? timeo { get; set; }   // 68/106
        [BsonIgnoreIfNull] public UInt32? vers { get; set; }    // 68/106
        [BsonIgnoreIfNull] public UInt32? wsize { get; set; }   // 68/106
        [BsonIgnoreIfNull] public string clientaddr { get; set; }       // 62/106
        [BsonIgnoreIfNull] public UInt32? stripe { get; set; }  // 12/106
        [BsonIgnoreIfNull] public string compress { get; set; } // 6/106
        [BsonIgnoreIfNull] public string mountaddr { get; set; }        // 6/106
        [BsonIgnoreIfNull] public UInt32? mountport { get; set; }       // 6/106
        [BsonIgnoreIfNull] public string mountproto { get; set; }       // 6/106
        [BsonIgnoreIfNull] public UInt32? mountvers { get; set; }       // 6/106
        [BsonIgnoreIfNull] public bool? space_cache { get; set; }       // 6/106
        [BsonIgnoreIfNull] public string subvol { get; set; }   // 6/106
        [BsonIgnoreIfNull] public UInt32? subvolid { get; set; }        // 6/106
        [BsonIgnoreIfNull] public bool? attr2 { get; set; }     // 2/106
        [BsonIgnoreIfNull] public bool? inode64 { get; set; }   // 2/106
        [BsonIgnoreIfNull] public string logbsize { get; set; } // 2/106
        [BsonIgnoreIfNull] public UInt32? logbufs { get; set; } // 2/106
        [BsonIgnoreIfNull] public bool? noquota { get; set; }   // 2/106
        [BsonIgnoreIfNull] public bool? relatime { get; set; }  // 2/106
    }
    public class DiskWatcher_MountInfo
    {
        [BsonIgnoreIfNull] public string Device { get; set; }   // 94/106
        [BsonIgnoreIfNull] public DiskWatcher_OptionInfo Options { get; set; }        // 94/106
        [BsonIgnoreIfNull] public string Point { get; set; }    // 94/106
        [BsonIgnoreIfNull] public string Target { get; set; }   // 94/106
        [BsonIgnoreIfNull] public string Type { get; set; }     // 94/106
    }

    public class DiskWatcher_HostCapture
    {
        [BsonIgnoreIfNull] public BsonDocument Error { get; set; }      // 94/106
        [BsonIgnoreIfNull] public BsonDocument ErrorTime { get; set; }  // 94/106
        [BsonIgnoreIfNull] public double? LastDuration { get; set; }    // 94/106
        [BsonIgnoreIfNull] public double? LastReport { get; set; }      // 94/106
        [BsonIgnoreIfNull] public bool? Master { get; set; }    // 94/106
        [BsonIgnoreIfNull] public DiskWatcher_MountInfo Mount { get; set; }  // 94/106
        [BsonIgnoreIfNull] public UInt32? PID { get; set; }     // 94/106
        [BsonIgnoreIfNull] public DiskWatcher_StatVFS StatVFS { get; set; }        // 94/106
    }


    public class DiskWatcher_Mount
    {
        public string Device { get; set; }      // 100/106
        public DiskWatcher_OptionInfo Options { get; set; }   // 100/106
        public string Point { get; set; }       // 100/106
        public string Target { get; set; }      // 100/106
        public string Type { get; set; }        // 100/106
    }
    public class DiskWatcher_StatVFS
    {
        public UInt32? f_bavail { get; set; }   // 100/106
        public UInt32? f_bfree { get; set; }    // 100/106
        public UInt64? f_blocks { get; set; }   // 100/106
        public UInt32? f_bsize { get; set; }    // 100/106
        public UInt32? f_favail { get; set; }   // 100/106
        public UInt32? f_ffree { get; set; }    // 100/106
        public UInt32? f_files { get; set; }    // 100/106
        public UInt32? f_flag { get; set; }     // 100/106
        public UInt32? f_frsize { get; set; }   // 100/106
        public UInt32? f_namemax { get; set; }  // 100/106
    }
    public class DiskWatcher
    {
        public string _id { get; set; }
        [BsonRequired] public double? FirstSeen { get; set; }   // 106/106
        [BsonRequired] public Dictionary<string, DiskWatcher_HostCapture> Hosts { get; set; }      // 106/106
        [BsonRequired] public double? LastReport { get; set; }  // 106/106
        [BsonRequired] public double? LastUpdate { get; set; }  // 106/106
        [BsonRequired] public string Name { get; set; } // 106/106
        public string HostedOn { get; set; }    // 100/106
        public DiskWatcher_Mount Mount { get; set; }     // 100/106
        public DiskWatcher_StatVFS StatVFS { get; set; }   // 100/106
        [BsonIgnoreIfNull] public UInt32? BakedStatus { get; set; }     // 88/106

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }

}
