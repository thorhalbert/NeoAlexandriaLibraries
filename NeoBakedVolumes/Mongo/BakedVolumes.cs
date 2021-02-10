using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

namespace NeoBakedVolumes.Mongo
{
    public static class BakedVolumes_exts
    {
        public static IMongoCollection<BakedVolumes> BakedVolumes(this IMongoDatabase db)
        {
            return db.GetCollection<BakedVolumes>("BakedVolumes");
        }
    }
    public class BakedVolumes
    {
        public string _id { get; set; }
        [BsonRequired] public UInt64 ArchSize { get; set; }    // 47598/47598
        [BsonRequired] public DateTime Created { get; set; }   // 47598/47598
        [BsonRequired] public bool DoECC { get; set; } // 47598/47598
        [BsonRequired] public UInt32 NumParts { get; set; }    // 47598/47598
        //[BsonRequired] public BakedVolumes_Parts Parts { get; set; } // 47598/47598
        [BsonRequired] public Dictionary<string, BakedVolumes_PartValues> Parts { get; set; }
        [BsonRequired] public string VolumeClass { get; set; }  // 47598/47598
        [BsonRequired] public bool Annealed { get; set; }  // 47586/47598
        [BsonIgnoreIfNull] public UInt32? TotalFiles { get; set; }  // 30802/47598
        [BsonIgnoreIfNull] public string ProcStatus { get; set; }   // 30794/47598
        [BsonIgnoreIfNull] public double? RepairLevel { get; set; } // 476/47598
        [BsonIgnoreIfNull] public bool? Repairable { get; set; }    // 70/47598
        [BsonIgnoreIfNull] public BakedVolumes_RepairStatuses RepairStatus { get; set; }  // 60/47598

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }

    public class BakedVolumes_PartValues
    {
        public class IntegerCoercion : SerializerBase<Int32>
        {
            public override Int32 Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                //Console.WriteLine($"Current: {context.Reader.CurrentBsonType}");


                if (context.Reader.CurrentBsonType == BsonType.Int32)
                {
                    var ret = context.Reader.ReadInt32();
                    //Console.WriteLine($"Value: {ret}");
                    return ret;
                }
                if (context.Reader.CurrentBsonType == BsonType.String)
                {
                    var value = context.Reader.ReadString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return 0;
                    }

                    var rint = Int32.Parse(value);

                    //Console.WriteLine($"Value String {value}, int {rint}");

                    return rint;
                }
                //Console.WriteLine($"Skipvalue");
                context.Reader.SkipValue();
                return 0;
            }

            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Int32 value)
            {
                context.Writer.WriteInt32(Convert.ToInt32(value));
            }
        }

        [BsonRequired] public double? DiceRoll { get; set; }    // 47598/47598
        [BsonRequired] public BakedVolumes_Hashes Hashes { get; set; }    // 47598/47598

        [BsonElement("Length")]
        [BsonSerializer(typeof(IntegerCoercion))]
        [BsonRequired] public Int32 Length { get; set; }  // 47598/47598
    
        [BsonRequired] public string Name { get; set; } // 47598/47598
        [BsonRequired] public string Path { get; set; } // 47598/47598
        [BsonRequired] public string Pool { get; set; } // 47598/47598
        [BsonRequired] public double? Scrub { get; set; }   // 47598/47598
        [BsonRequired] public double? ScrubOn { get; set; } // 47598/47598
        [BsonRequired] public string State { get; set; }    // 47598/47598
        [BsonIgnoreIfNull] public bool? Bad { get; set; }   // 40994/47598
        [BsonIgnoreIfNull] public string Problem { get; set; }  // 40994/47598
        [BsonIgnoreIfNull] public string Tool { get; set; } // 26344/47598

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }
 
    public class BakedVolumes_Parts
    {
        [BsonRequired] public BakedVolumes_PartValues A { get; set; } // 47598/47598
        [BsonRequired] public BakedVolumes_PartValues B { get; set; } // 47598/47598
        [BsonRequired] public BakedVolumes_PartValues C { get; set; } // 47598/47598
        [BsonRequired] public BakedVolumes_PartValues D { get; set; } // 47598/47598
        [BsonRequired] public BakedVolumes_PartValues X { get; set; } // 47598/47598
        [BsonRequired] public BakedVolumes_PartValues Z { get; set; } // 47598/47598
    }
  
    public class BakedVolumes_RepairStatuses
    {
        [BsonIgnoreIfNull] public double? Attempt { get; set; } // 60/47598
        [BsonIgnoreIfNull] public string Tool { get; set; } // 60/47598
    }

 
    public class BakedVolumes_Hashes
    {
        [BsonRequired] public string MD5 { get; set; }  // 47598/47598
        [BsonRequired] public string SHA1 { get; set; } // 47598/47598
        [BsonRequired] public string SHA256 { get; set; }   // 47598/47598
        [BsonRequired] public string TIGER { get; set; }    // 47598/47598
        [BsonRequired] public string WHIRLPOOL { get; set; }    // 47598/47598
    }
  
}
