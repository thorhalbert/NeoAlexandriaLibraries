using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace NeoCuratedCollections.Mongo
{
    class CurationManifest
    {
        public Guid _id { get; set; }
        public Guid TopId { get; set; }
        public int TopSeq { get; set; }
        List<CuratedMembers> Members { get;set;}
    }
}
