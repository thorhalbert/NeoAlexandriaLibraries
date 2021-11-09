using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PenguinSanitizer;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Linux;

namespace NeoAssets.Mongo
{
    public static class NeoVirtFS_exts
    {
        public static IMongoCollection<NeoVirtFS> NeoVirtFS(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFS>("NeoVirtFS");
        }
        public static IMongoCollection<NeoVirtFSNamespaces> NeoVirtFSNamespaces(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSNamespaces>("NeoVirtFSNamespaces");
        }
        public static IMongoCollection<NeoVirtFSVolumes> NeoVirtFSVolumes(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSVolumes>("NeoVirtFSVolumes");
        }
    }
    public class NeoVirtFS
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonRequired] public byte[] Name { get; set; }

        [BsonRequired] public NeoVirtFSStat Stat { get; set; }

        [BsonRequired] public byte[] AssetSHA1 { get; set; }        // File is annealed - hardlink to virtual asset - if > 0 bytes

        // Going to go with a doubly linked list for now.   Need an fsck to fix - probably not much to do with how inodes work on regular filesystems

        [BsonRequired] public ObjectId ParentId { get; set; }
        [BsonRequired] public ObjectId[] MemberIds { get; set; }  // Child nodes (of directory)

        [BsonIgnoreIfNull] public ObjectId MountedVolume { get; set; }
        [BsonIgnoreIfNull] public ObjectId AtFilePath { get; set; }  // Starting directory

        [BsonIgnoreIfNull] public byte[] PhysicalFile { get; set; }  // Linkage to physicalfile
        [BsonIgnoreIfNull] public byte[] CacheFile { get; set; }    // Path to physical cached file

        [BsonRequired] public NeoVirtFSAttributes Attributes { get; set; }
    }

    public class NeoVirtFSAttributes
    {
        [BsonExtraElements] public BsonDocument Elements { get; set; }  // Catch the attributes
    }

    public class NeoVirtFSStat
    {
        public NeoVirtFSStat() { }
        public NeoVirtFSStat(stat st)
        {
            st_size = Convert.ToUInt64(st.st_size);

            st_uid = st.st_uid;
            st_gid = st.st_gid;

            st_mode = (NeoMode_T) Convert.ToUInt32(st.st_mode);

            st_ctim = st.st_ctim.ToDTO();
            st_mtim = st.st_mtim.ToDTO();
            st_atim = st.st_atim.ToDTO();
            st_dtim = DateTimeOffset.MinValue;
        }

        [BsonRequired] public UInt64 st_size;

        // This until we have more complex security constructs
        [BsonRequired] public UInt32 st_uid;
        [BsonRequired] public UInt32 st_gid;

        [BsonRequired] public NeoMode_T st_mode;

        [BsonRequired] public DateTimeOffset st_ctim;
        [BsonRequired] public DateTimeOffset st_mtim;
        [BsonRequired] public DateTimeOffset st_atim;
        [BsonRequired] public DateTimeOffset st_dtim;
    }

    public class NeoVirtFSNamespaces        // First level of heirarchy
    {
        [BsonId] public ObjectId _id { get; set; }              // ObjectId of namespace
        [BsonRequired] public string Namespace { get; set; }    // Namespace (Unicode legal string)
        [BsonRequired] public ObjectId NodeId { get; set; }    // Pointer to NeoVirtFS Directory Node
        [BsonIgnoreIfNull] public bool Root { get; set; }       // Is this element the root of the tree
    }

    public class NeoVirtFSVolumes           // Second level of heirarchy
    {
        [BsonId] public ObjectId _id { get; set; }              // ObjectId of volume
        [BsonRequired] public ObjectId NameSpace { get; set; }  // ObjectId of namespace
        [BsonRequired] public string Name { get; set; }         // Name of Volume
        [BsonRequired] public ObjectId NodeId { get; set; }    // Pointer to NeoVirtFS Directory Node
    }

    //public class NeoVirtFSSecPrincipals
    //{
    //    [BsonId] public ObjectId _id { get; set; }
    //}
}
