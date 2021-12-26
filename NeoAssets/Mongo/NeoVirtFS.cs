﻿using Logos.Utility;
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

        public static IMongoCollection<NeoVirtFS> NeoVirtFSDeleted(this IMongoDatabase db)
        {
            var col = db.GetCollection<NeoVirtFS>("NeoVirtFSDeleted");

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
        public static IMongoCollection<NeoVirtFSSecPrincipals> NeoVirtFSSecPrincipals(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSSecPrincipals>("NeoVirtFSSecPrincipals");
        }
        public static IMongoCollection<NeoVirtFSSecACL> NeoVirtFSSecACLs(this IMongoDatabase db)
        {
            return db.GetCollection<NeoVirtFSSecACL>("NeoVirtFSSecACLs");
        }
    }
    public class NeoVirtFS
    {
        [BsonId]
        [BsonRequired] public ObjectId _id { get; set; }
        [BsonRequired] public ObjectId NameSpace { get; set; }
        [BsonRequired] public ObjectId ParentId { get; set; }  // [Indexed] only rely on these (members are for repair hints)
        [BsonIgnoreIfNull] public ObjectId[] MemberIds { get; set; }  // Child nodes (of directory) - let's stick with parentId for now

        [BsonRequired] public byte[] Name { get; set; }         // [Indexed]

        [BsonRequired] public NeoVirtFSStat Stat { get; set; }

        [BsonRequired] public NeoVirtFSContent Content { get; set; }

        [BsonIgnoreIfNull] public ObjectId? DirectoryDefaultACL { get; set; }  // Override Default ACL for tree down
        [BsonIgnoreIfNull] public ObjectId? FileACL { get; set; }            // Non-default ACL for file

        // These simply might get pulled from the asset (or maybe here if get set via xattr calls)
        [BsonIgnoreIfNull] public NeoVirtFSAttributes Attributes { get; set; }

        [BsonRequired] public bool MaintLevel { get; set; }  // If set, is at the maintenance levels, user's can't create things

        public void GetStat(ref stat stat)
        {
            Stat.GetStat(ref stat);
        }

        public static NeoVirtFS CreateNewFile(NeoVirtFS par, byte[] name, ReadOnlySpan<byte> path, mode_t mode)
        {
            var newRec = new NeoVirtFS
            {
                _id = new ObjectId(),
                Name = name,
                NameSpace = par.NameSpace,
                ParentId = par._id,
                Stat = NeoVirtFSStat.FileDefault((uint) mode),
                Content = NeoVirtFSContent.NewCache(path),
                MaintLevel = false
            };

            return newRec;
        }
    }

    public enum VirtFSContentTypes
    {
        NotAFile = 10,
        Asset = 20,
        MountedVolume = 30,
        PhysicalFile = 40,
        CachePool = 50,
    }

    public class NeoVirtFSContent
    {
        [BsonRequired] public VirtFSContentTypes ContentType { get; set; }

        // NotAFile = 10
        [BsonIgnoreIfNull] public bool NotAFile { get; set; }           // Also for empty files (though there is a sha1 for that)

        // Asset = 20
        [BsonIgnoreIfNull] public byte[] AssetSHA1 { get; set; }        // File is annealed - hardlink to virtual asset - if > 0 bytes

        // MountedVolume = 30
        [BsonIgnoreIfNull] public ObjectId? MountedVolume { get; set; }  // Link (like symbolic link) go other volume
        [BsonIgnoreIfNull] public ObjectId? AtFilePath { get; set; }  // Starting object (object within MountedVolume)

        // PhysicalFile = 40,
        [BsonIgnoreIfNull] public byte[] PhysicalFile { get; set; }  // Linkage (split) to physicalfile (though for NARPS they should be the /NARP path)

        // CachePool = 50,
      
        // It was a tossup between using an objectId here and a guid.  
        // Guid was more positional, so seemed a better way to generate an id
        [BsonIgnoreIfNull] public Guid? CachePoolGuid { get; set; }
        [BsonIgnoreIfNull] public byte[] CacheFile { get; set; }    // serialized version of CachePoolGuid, full path, split

        public static NeoVirtFSContent Dir()
        {
            var ret = new NeoVirtFSContent
            {
                ContentType = VirtFSContentTypes.NotAFile,
                NotAFile = true
            };

            return ret;
        }

        internal static NeoVirtFSContent NewCache(ReadOnlySpan<byte> path)
        {
            var fileuuid = GuidUtility.Create(GuidUtility.UrlNamespace, path.ToArray());

            var fileg = fileuuid.ToString().ToLower();

            // 8478e36a-669c-11ec-8541-17df4f57891e
            // 0123456789012345678901234567890123456789
            // 000000000011111111112222222222333333333

            // Ultimately will need real pool mechanism here

            var cacheFile = $"/ua/NeoVirtCache/{fileg.Substring(0,2)}/{fileg.Substring(2,2)}/{fileg.Substring(4,2)}/{fileg.Substring(6,2)}/{fileg}";

            var rec = new NeoVirtFSContent
            {
                CachePoolGuid = fileuuid,
                ContentType = VirtFSContentTypes.CachePool,
                CacheFile = Encoding.UTF8.GetBytes(cacheFile)
            };

            return rec;
        }
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

        public static NeoVirtFSStat DirDefault(uint mode = 0b111_101_101)
        {        
            var stt = DateTimeOffset.UtcNow;

            var st = new NeoVirtFSStat()
            {
                st_size = 0,
                st_mode = NeoMode_T.S_IFDIR | (NeoMode_T) mode,  // 755
                st_uid = 10010,
                st_gid = 10010,
                st_atim = stt,
                st_ctim = stt,
                st_mtim = stt,
                st_dtim = DateTimeOffset.MinValue,
            };

            return st;
        }

        internal void GetStat(ref stat stat)
        {
            stat.st_nlink = 1;

            stat.st_size = Convert.ToInt64(st_size);

            stat.st_uid = st_uid;
            stat.st_gid = st_gid;

            stat.st_mode = st_mode.GetMode();

            stat.st_atim = st_atim.GetTimeSpec();
            stat.st_ctim = st_ctim.GetTimeSpec();
            stat.st_mtim = st_mtim.GetTimeSpec();
        }

        internal static NeoVirtFSStat FileDefault(uint mode = 0b110_100_100)
        {
            var stt = DateTimeOffset.UtcNow;

            var st = new NeoVirtFSStat()
            {
                st_size = 0,
                st_mode = NeoMode_T.S_IFREG | (NeoMode_T) mode,  // 644
                st_uid = 10010,
                st_gid = 10010,
                st_atim = stt,
                st_ctim = stt,
                st_mtim = stt,
                st_dtim = DateTimeOffset.MinValue,
            };

            return st;
        }
    }

    public class NeoVirtFSNamespaces        // First level of heirarchy
    {
        [BsonId] public ObjectId _id { get; set; }              // ObjectId of namespace
        [BsonRequired] public string NameSpace { get; set; }    // Namespace (Unicode legal string)
        [BsonRequired] public ObjectId NodeId { get; set; }    // Pointer to NeoVirtFS Directory Node
        [BsonRequired] public ObjectId ParentId { get; set; }    // Pointer to NeoVirtFS Directory Node
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
