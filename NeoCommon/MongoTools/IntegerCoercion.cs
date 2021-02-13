using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCommon.MongoTools
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

}
