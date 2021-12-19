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
        private static bool _NeoVirtFSSet = false;
        public static IMongoCollection<NeoVirtFS> NeoVirtFS(this IMongoDatabase db)
        {
            var col = db.GetCollection<NeoVirtFS>("NeoVirtFS");

            if (!_NeoVirtFSSet)
            {
                // No reason for parent id to be anything but hashed
                var indexKeysDefinition = Builders<NeoVirtFS>.IndexKeys.Hashed(idx => idx.ParentId);
                col.Indexes.CreateOne(new CreateIndexModel<NeoVirtFS>(indexKeysDefinition));

                // For now filename access will also be hashed (no use-case for partial access)
                indexKeysDefinition = Builders<NeoVirtFS>.IndexKeys.Hashed(idx => idx.Name);
                col.Indexes.CreateOne(new CreateIndexModel<NeoVirtFS>(indexKeysDefinition));

                _NeoVirtFSSet = true;
            }

            return col;
        }
        public static IMongoCollection<NeoVirtFSNamespaces> NeoVirtFSNamespaces(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSNamespaces>("NeoVirtFSNamespaces");
        }
        public static IMongoCollection<NeoVirtFSVolumes> NeoVirtFSVolumes(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSVolumes>("NeoVirtFSVolumes");
        }
        public static IMongoCollection<NeoVirtFSVolumes> NeoVirtFSSecPrincipals(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSVolumes>("NeoVirtFSSecPrincipals");
        }
        public static IMongoCollection<NeoVirtFSVolumes> NeoVirtFSSecACLs(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSVolumes>("NeoVirtFSSecACLs");
        }
    }
    public class NeoVirtFS
    {
        [BsonId]
        [BsonRequired] public ObjectId _id { get; set; }
        [BsonRequired] public ObjectId NameSpace { get; set; }
        [BsonRequired] public ObjectId ParentId { get; set; }  // [Indexed] only rely on these (members are for repair hints)
        [BsonRequired] public ObjectId[] MemberIds { get; set; }  // Child nodes (of directory)

        [BsonRequired] public byte[] Name { get; set; }         // [Indexed]

        [BsonRequired] public NeoVirtFSStat Stat { get; set; }

        [BsonRequired] public NeoVirtFSContent Content { get; set; }

        [BsonIgnoreIfNull] public ObjectId? DirectoryDefaultACL { get; set; }  // Override Default ACL for tree down
        [BsonIgnoreIfNull] public ObjectId? FileACL { get; set; }            // Non-default ACL for file

        // These simply might get pulled from the asset (or maybe here if get set via xattr calls)
        [BsonRequired] public NeoVirtFSAttributes Attributes { get; set; }
    }

    public class NeoVirtFSContent
    {
        [BsonRequired] public bool NotAFile { get; set; }           // Also for empty files (though there is a sha1 for that)
        [BsonRequired] public byte[] AssetSHA1 { get; set; }        // File is annealed - hardlink to virtual asset - if > 0 bytes

        [BsonIgnoreIfNull] public ObjectId? MountedVolume { get; set; }  // Link (like symbolic link) go other volume
        [BsonIgnoreIfNull] public ObjectId? AtFilePath { get; set; }  // Starting object (object within MountedVolume)

        [BsonIgnoreIfNull] public byte[][] PhysicalFile { get; set; }  // Linkage (split) to physicalfile (though for NARPS they should be the /NARP path)

        [BsonIgnoreIfNull] public ObjectId? CachePool { get; set; }
        [BsonIgnoreIfNull] public byte[][] CacheFile { get; set; }    // serialized version of objectId, full path, split
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
        [BsonRequired] public DateTimeOffset st_dtim;   // When we notice it's deleted
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
        [BsonIgnoreIfNull] public ObjectId? VolumeDefaultACL { get; set; }
    }

    public enum NeoVirtFSSecPrincipalTypes
    {
        Owner = 1,
        Role = 2,
    }
    public class NeoVirtFSSecPrincipals
    {
        [BsonId] public ObjectId _id { get; set; }
        [BsonRequired] public NeoVirtFSSecPrincipalTypes PrincipalType {get;set;}
        // Roles assigned to Principal (can also be roles assigned to roles, recursively)
        // Ultimately this is a union of all
        [BsonRequired] public ObjectId[] Roles { get; set; }
    }

    [Flags]
    public enum NeoVirtFSSecACLOperations
    {
        Read = 4,
        Write = 2,
        Execute = 1,

        // Put system into different modes (presents helper links) - mostly mapping to narp type
        MirrorOperation = 8,    // Don't present helpful links to production (so mirror doesn't get confused)
        SeeDeleted = 16,        // Show deleted files (with flag)
        SeeArchives = 32,       // Next to archive will be a linkfile to archive extract
        NoDeleted = 64,         // Don't show deleted files
    }
    public class NeoVirtFSSecACLElements
    {
        [BsonId] public ObjectId Principal { get; set; }
        [BsonRequired] public bool Allow { get; set; }  // Default Deny
        [BsonRequired] public int Operation { get; set; }  // This will be enum flags
    }
    public class NeoVirtFSSecACL
    {
        [BsonId] public ObjectId _id { get; set; }
        [BsonRequired] public NeoVirtFSSecACLElements[] Elements { get; set; }
        [BsonRequired] public bool DefaultAllow { get; set; }  // Default Deny
    }
}
