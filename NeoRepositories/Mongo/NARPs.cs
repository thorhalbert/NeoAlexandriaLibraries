using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoRepositories.Mongo
{
    public static class NARPs_exts
    {
        public static IMongoCollection<NARPs> NARPs(this IMongoDatabase db)
        {
            return db.GetCollection<NARPs>("NARPs");
        }
    }
    public class NARPs
    {
        public static NARPs GetNARP(IMongoDatabase db, string narp)
        {
            var nd = db.GetCollection<NARPs>("NARPs");

            return nd.FindSync(Builders<NARPs>.Filter.Eq("_id", narp)).FirstOrDefault();
        }

        public string _id { get; set; }
        [BsonIgnoreIfNull] public string LastPhysical { get; set; } // 7824/7824
        [BsonIgnoreIfNull] public DateTime? LastSeen { get; set; }  // 7824/7824
        [BsonIgnoreIfNull] public NARPS_Processing Processing { get; set; }   // 7820/7824
        [BsonIgnoreIfNull] public bool? Dirty { get; set; } // 7818/7824
        [BsonIgnoreIfNull] public DateTime? LastGrind { get; set; } // 7818/7824
        [BsonIgnoreIfNull] public bool? OvenLoaded { get; set; }    // 7818/7824
        [BsonIgnoreIfNull] public DateTime? ScryFile_End { get; set; }  // 7816/7824
        [BsonIgnoreIfNull] public UInt32? ScryFile_Errors { get; set; } // 7816/7824
        [BsonIgnoreIfNull] public DateTime? ScryFile_Start { get; set; }    // 7816/7824
        [BsonIgnoreIfNull] public BsonDocument Operations { get; set; }   // 7814/7824
        [BsonIgnoreIfNull] public DateTime? ScryTotals_End { get; set; }    // 7810/7824
        [BsonIgnoreIfNull] public NARPS_Totals Totals { get; set; }   // 7810/7824
        [BsonIgnoreIfNull] public DateTime? FirstSeen { get; set; } // 7794/7824
        [BsonIgnoreIfNull] public List<string> Keywords { get; set; }   // 6144/7824
        [BsonIgnoreIfNull] public NARPS_NeoScanAll NeoScanAll { get; set; }   // 6058/7824
        [BsonIgnoreIfNull] public double? ScanDirty { get; set; }   // 3396/7824
        [BsonIgnoreIfNull] public double? BakePriority { get; set; }    // 2530/7824
        [BsonIgnoreIfNull] public double? RemoveDir { get; set; }   // 1848/7824
        [BsonIgnoreIfNull] public string Style { get; set; }    // 1848/7824
        [BsonIgnoreIfNull] public BsonDocument StyleDirs { get; set; }    // 1848/7824
        [BsonIgnoreIfNull] public string Owner { get; set; }    // 708/7824
        [BsonIgnoreIfNull] public bool? Backup { get; set; }    // 240/7824
        [BsonIgnoreIfNull] public string NARPType { get; set; } // 222/7824
        [BsonIgnoreIfNull] public bool? AlwaysDirty { get; set; }   // 104/7824
        [BsonIgnoreIfNull] public double? Cycle { get; set; }   // 104/7824
        [BsonIgnoreIfNull] public List<string> Pools { get; set; }  // 34/7824
        [BsonIgnoreIfNull] public string Universe { get; set; } // 26/7824
        [BsonIgnoreIfNull] public bool? ExcludeProcess { get; set; }    // 14/7824
        [BsonIgnoreIfNull] public BsonDocument Assets { get; set; }   // 4/7824
        [BsonIgnoreIfNull] public NARPS_Paths Paths { get; set; }    // 4/7824
        [BsonIgnoreIfNull] public BsonDocument Summary { get; set; }  // 4/7824
        [BsonIgnoreIfNull] public double? Duration { get; set; }    // 2/7824
        [BsonIgnoreIfNull] public DateTime? Finished { get; set; }  // 2/7824
        [BsonIgnoreIfNull] public bool? Hold { get; set; }  // 2/7824
        [BsonIgnoreIfNull] public string HoldReason { get; set; }   // 2/7824

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }

    public class NARPS_Totals_Content
    {
        [BsonIgnoreIfNull] public NARPS_SummaryTotals Baked { get; set; }    // 7810/7824
        [BsonIgnoreIfNull] public NARP_Totals Deleted { get; set; }  // 7810/7824
        [BsonIgnoreIfNull] public NARP_Totals Physical { get; set; } // 7810/7824
    }
    public class NARPS_NeoScanAll
    {
        [BsonIgnoreIfNull] public DateTime? Ended { get; set; } // 6058/7824
        [BsonIgnoreIfNull] public DateTime? Started { get; set; }   // 6058/7824
        [BsonIgnoreIfNull] public string State { get; set; }    // 6058/7824
        [BsonIgnoreIfNull] public DateTime? FullScan { get; set; }  // 5958/7824
    }
    public class NARPs_39a1d6a2_2c81_50e8_b622_ef352a461802
    {
        [BsonIgnoreIfNull] public DateTime? Finished { get; set; }  // 6744/7824
    }

    public class NARP_Totals
    {
        [BsonIgnoreIfNull] public UInt32? Directories { get; set; } // 7810/7824
        [BsonIgnoreIfNull] public UInt32? Files { get; set; }   // 7810/7824
        [BsonIgnoreIfNull] public UInt64? MaxDate { get; set; } // 7810/7824
        [BsonIgnoreIfNull] public UInt32? MinDate { get; set; } // 7810/7824
        [BsonIgnoreIfNull] public UInt64? TotalSize { get; set; }   // 7810/7824
    }
 
    public class NARPS_Totals
    {
        [BsonIgnoreIfNull] public NARPS_Totals_Archive Archive { get; set; }  // 7810/7824
        [BsonIgnoreIfNull] public NARPS_Totals_Content Content { get; set; }  // 7810/7824
        [BsonIgnoreIfNull] public DateTime? Taken { get; set; } // 7810/7824
    }

    
    public class NARPS_SummaryTotals
    {
        [BsonIgnoreIfNull] public UInt32? Directories { get; set; } // 7810/7824
        [BsonIgnoreIfNull] public UInt32? Files { get; set; }   // 7810/7824
        [BsonIgnoreIfNull] public UInt32? MaxDate { get; set; } // 7810/7824
        [BsonIgnoreIfNull] public UInt32? MinDate { get; set; } // 7810/7824
        [BsonIgnoreIfNull] public UInt64? TotalSize { get; set; }   // 7810/7824
    }
 
    public class NARP_Processing_FollupQueue
    {
        [BsonIgnoreIfNull] public string Action { get; set; }   // 2/7824
        [BsonIgnoreIfNull] public BsonDocument Finished { get; set; }   // 2/7824
        [BsonIgnoreIfNull] public BsonDocument Issues { get; set; } // 2/7824
        [BsonIgnoreIfNull] public BsonDocument Started { get; set; }    // 2/7824
        [BsonIgnoreIfNull] public BsonDocument Status { get; set; } // 2/7824
    }
    public class NARPS_Processing
    {
        [BsonIgnoreIfNull] public BsonDocument Finished { get; set; }   // 7820/7824
        [BsonIgnoreIfNull] public BsonDocument Followup { get; set; }   // 7820/7824
        [BsonIgnoreIfNull] public List<NARP_Processing_FollupQueue> Followup_Queue { get; set; } // 7820/7824
        [BsonIgnoreIfNull] public BsonDocument Latest { get; set; } // 7820/7824
        [BsonIgnoreIfNull] public string Policy { get; set; }   // 7820/7824
        [BsonIgnoreIfNull] public List<BsonDocument> Queue { get; set; }  // 7820/7824
        [BsonIgnoreIfNull] public DateTime? Queued { get; set; }    // 7820/7824
        [BsonIgnoreIfNull] public UInt32? Version { get; set; } // 7820/7824

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }
    public class NARPS_Totals_Archive
    {
        [BsonIgnoreIfNull] public NARP_Totals Baked { get; set; }    // 7810/7824
        [BsonIgnoreIfNull] public NARP_Totals Deleted { get; set; }  // 7810/7824
        [BsonIgnoreIfNull] public NARP_Totals Physical { get; set; } // 7810/7824
    }
    public class NARPS_Paths
    {
        [BsonIgnoreIfNull] public string MOVIES { get; set; }   // 4/7824
    }

}
