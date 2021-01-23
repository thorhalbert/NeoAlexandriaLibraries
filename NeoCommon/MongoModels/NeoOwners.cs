using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCommon.MongoModels
{
    public class NeoOwners
    {
        public string _id { get; set; }
        [BsonRequired] public string BakeRoot { get; set; } // 8/8
        [BsonRequired] public bool Default { get; set; }   // 8/8
        [BsonRequired] public string Description { get; set; }  // 8/8
        [BsonRequired] public string Ident { get; set; }    // 8/8
        [BsonRequired] public string PoolDir { get; set; }  // 8/8
        [BsonIgnoreIfNull] public string BakedClass { get; set; }   // 4/8

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }

}
