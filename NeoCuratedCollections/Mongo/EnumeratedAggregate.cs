using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCuratedCollections.Mongo
{
    public class EnumeratedAggregate
    {
        /*
        * Contents array
            * Range 
              * Array [ sourcemap, first, last ]
            * Select 
              * Array [odd,even,...] - page pairs, odd is map index, even is key (either a page #, or a CB? filename)
            * Seq - page # or whatever - allows multiple items on same page - If range, then next X are implied
            * Rotate - 90 degree rotations - 0 up, clockwise
            * Position: x,y
            * Scale: double
            * Flip - flip over axis from angle, 0 up, clockwise
            * Up - original up angle (page that's 90 degress for example) - 0 up, clockwise
            * Crop - Pos + Bounds
            * Clip - clip against polygonal boundary
            * Z - stack order for page
            * Attribs - subobject which has artifact level values (like page #)
         */

        public ushort[] Map { get; set; }
        public byte[][] Assets { get; set; }
        [BsonIgnoreIfNull]
        public uint[] Range { get; set; }
        [BsonIgnoreIfNull]
        public uint[] Select { get; set; }
        public object Seq { get; set; }
        [BsonIgnoreIfNull]
        public double? Rotate { get; set; }
        [BsonIgnoreIfNull]
        public double? Scale { get; set; }
        [BsonIgnoreIfNull]
        public double? Flip { get; set; }
        [BsonIgnoreIfNull]
        public double? Up { get; set; }
        [BsonIgnoreIfNull]
        public double? Z { get; set; }
        [BsonIgnoreIfNull]
        public double[] Clip { get; set; }
        [BsonIgnoreIfNull]
        public double[] Crop { get; set; }
        [BsonIgnoreIfNull]
        public double[] Position { get; set; }
        [BsonIgnoreIfNull]
        public BsonDocument ArtifactValues { get; set; }
    }
}
