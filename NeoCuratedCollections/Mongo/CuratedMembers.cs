using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCuratedCollections.Mongo
{
    public class CuratedMembers
    {
        /*
         *  * S - sequence #
              * AR - Artifact Type Object
                * Art: Artifact Key (binary)
                * ...  artifact private parameters
              * CR - Start Collection (binary key)
              * CI - Collection information (string)
              * CS - collection sequence
              * CD - collection depth
              * A - Import asset (binary key)
              * C - Import other collection
         */

        public Guid AR { get; set; }
        public Guid Art { get; set; }
        [BsonIgnoreIfNull]
        public BsonDocument ArtOpts { get; set; }
        [BsonIgnoreIfNull]
        public Guid? CR { get; set; }
        [BsonIgnoreIfNull]
        public string CI { get; set; }
        public uint CS { get; set; }
        public ushort CD { get; set; }
        [BsonIgnoreIfNull]
        public Byte[] A { get; set; }
        [BsonIgnoreIfNull]
        List<EnumeratedAggregate> AG { get; set; }
    }
}
