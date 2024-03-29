﻿using Logos.Utility;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PenguinSanitizer;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Linux;
using NeoRepositories.Mongo;
using System.Linq;
using NeoCommon;
using NeoBakedVolumes.Mongo;
using static NeoAssets.Mongo.NeoVirtFS;
using NeoVirtFS.Events;
using SharpCompress.Compressors.Deflate;

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

    public enum DeleteTypes
    {
        TRUNC = 1,
        UNLINK = 2,
        CREATE = 3,
        RENAMEOVER = 4,
        RMDIR = 5,
        LOST = 99,
    }
    public class NeoVirtFS
    {
        [BsonId]
        [BsonRequired] public ObjectId _id { get; set; }
        [BsonRequired] public ObjectId VolumeId { get; set; }   // What Volume are we in
        [BsonRequired] public ObjectId ParentId { get; set; }  // [Indexed] only rely on these (members are for repair hints)
        [BsonIgnoreIfNull] public ObjectId[] MemberIds { get; set; }  // Child nodes (of directory) - let's stick with parentId for now

        [BsonRequired] public byte[] Name { get; set; }         // [Indexed]
        [BsonIgnoreIfNull] public int Version { get; set; } = 1;

        [BsonRequired] public NeoVirtFSStat Stat { get; set; }

        [BsonRequired] public NeoVirtFSContent Content { get; set; }

        [BsonIgnoreIfNull] public string NARPImport { get; set; }

        [BsonIgnoreIfNull] public ObjectId? DirectoryDefaultACL { get; set; }  // Override Default ACL for tree down
        [BsonIgnoreIfNull] public ObjectId? FileACL { get; set; }            // Non-default ACL for file
        [BsonIgnoreIfNull] public DeleteTypes DeleteType { get; set; }      // What are we deleted (also implies that we are deleted)


        // These simply might get pulled from the asset (or maybe here if get set via xattr calls)
        [BsonIgnoreIfNull] public NeoVirtFSAttributes Attributes { get; set; }

        [BsonRequired] public bool MaintLevel { get; set; }  // If set, is at the maintenance levels, user's can't create things
        public bool IsDirectory {
            get {
                return Stat.IsDirectory;
            }
        }
        public bool IsFile {
            get {
                return Stat.IsFile;
            }
        }

        public void GetStat(ref stat stat)
        {
            Stat.GetStat(ref stat);
        }
        public void GetStat(ref stat stat, stat nstat)
        {
            Stat.GetStat(ref stat, nstat);
        }

        public static void DoAnneal(IMongoDatabase db, string obj, string p, bool assetDebug)
        {
            var vir = db.NeoVirtFS();

            ObjectId id;
            try
            {
                id = ObjectId.Parse(obj);
            }
            catch
            {
                Console.WriteLine($"Bad object key {obj}");
                return;
            }

            try
            {

                var filter = Builders<NeoVirtFS>.Filter.Eq(x => x._id, id);
                var node = vir.FindSync(filter).FirstOrDefault();
                if (node == null)
                {
                    Console.WriteLine($"Can't find file node {id}");
                    var del = new byte[] { (byte) '.', (byte) 'o', (byte) 'l', (byte) 'd' };

                    if (p != null)
                    {
                        Console.WriteLine($"Mark Redundant cache file as old: {p}");
                        RenameFile(p, del);
                    }
                    return;
                }

                byte[] setSha1;
                if (!node.DeepAnneal(db, p, assetDebug, out setSha1))
                {
                    Console.WriteLine($"Could not Anneal: {id} {p}");
                    var eventRec = new Event_FileNeedsBaked
                    {
                        EventTime = DateTimeOffset.Now,
                        ServerName = Environment.MachineName,
                        VolumeId = node.VolumeId.ToString(),
                        FileId = node._id.ToString(),
                        AssetSha1 = setSha1
                    };

                    eventRec.SendMessage();
                    Console.WriteLine("SendMessage called");
                }
                else
                {
                    var eventRec = new Event_FileAnnealed
                    {
                        EventTime = DateTimeOffset.Now,
                        ServerName = Environment.MachineName,
                        VolumeId = node.VolumeId.ToString(),
                        FileId = node._id.ToString(),
                        AssetSha1 = setSha1
                    };

                    eventRec.SendMessage();
                }
            }
            catch (ZlibException zx)
            {
                Console.WriteLine($"UnbakeForFuse::ZlibException NeoVirtFS {zx.Message} {zx.StackTrace}");
                
            }
        }

        public static NeoVirtFS CreateDirectory(ObjectId parId, ObjectId volId, byte[] name, mode_t mode)
        {
            var newRec = new NeoVirtFS()
            {
                _id = ObjectId.GenerateNewId(),
                Content = NeoVirtFSContent.Dir(),
                Stat = NeoVirtFSStat.DirDefault((uint) mode),
                Name = name,
                VolumeId = volId,
                ParentId = parId,
                MaintLevel = false,
            };

            return newRec;
        }
        public static NeoVirtFS CreateNewFile(ObjectId parId, ObjectId volId, byte[] name, ReadOnlySpan<byte> path, mode_t mode, NeoVirtFSContent cont = null)
        {
            var id = ObjectId.GenerateNewId();

            if (cont == null)
                cont = NeoVirtFSContent.NewCache(path, id);

            //Console.WriteLine($"Set newId for file to {id}");
            var newRec = new NeoVirtFS
            {
                _id = id,
                Name = name,
                VolumeId = volId,
                ParentId = parId,
                Stat = NeoVirtFSStat.FileDefault((uint) mode),
                Content = cont,
                MaintLevel = false
            };

            return newRec;
        }

        public static bool PullNamespacesAndVolumes(IMongoDatabase db,
            ref Dictionary<string, NeoVirtFSNamespaces> NamespaceNames,
            ref Dictionary<ObjectId, NeoVirtFSNamespaces> Namespaces,
            ref NeoVirtFSNamespaces RootNameSpace)
        {
            bool HaveRoot = false;

            var NeoVirtFSCol = db.NeoVirtFS();
            var NeoVirtFSDeletedCol = db.NeoVirtFSDeleted();
            var NeoVirtFSNamespacesCol = db.NeoVirtFSNamespaces();
            var NeoVirtFSVolumesCol = db.NeoVirtFSVolumes();
            var NeoVirtFSSecPrincipalsCol = db.NeoVirtFSSecPrincipals();
            var NeoVirtFSSecACLsCol = db.NeoVirtFSSecACLs();

            // Prepare for bulk update

            var updates = new List<WriteModel<NeoAssets.Mongo.NeoVirtFS>>();

            // Load up the namespaces -- there just shouldn't be too many of these

            var nCount = ProcessNamespaces(db, NamespaceNames, Namespaces, ref RootNameSpace, ref HaveRoot, updates);
            if (!HaveRoot)
                throw new ApplicationException("Volume Namespaces don't define a Root - Setup Issue");

            // Now do volumes

            int vCount = 0;
            var volumes = NeoVirtFSVolumesCol.FindSync(Builders<NeoVirtFSVolumes>.Filter.Empty).ToList();
            foreach (var v in volumes)
            {
                //Console.WriteLine($"Volume: {v.Name}");
                vCount++;

                // Ensure that the filesystem nodes exist at the top level

                FilterDefinition<NeoVirtFS> filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, v._id);

                var upd = new UpdateDefinitionBuilder<NeoAssets.Mongo.NeoVirtFS>()
                    .Set("_id", v._id)
                    .SetOnInsert("Content", NeoVirtFSContent.Dir())
                    .SetOnInsert("Stat", NeoVirtFSStat.DirDefault())
                    .Set("VolumeId", v.NameSpace)
                    .Set("ParentId", Namespaces[v.NameSpace]._id) // Set's see what root does
                    .Set("Name", Encoding.UTF8.GetBytes(v.Name))
                    .Set("MaintLevel", false);    // This is the volume level - users can do stuff here (by their policy)

                UpdateOneModel<NeoVirtFS> update = new UpdateOneModel<NeoAssets.Mongo.NeoVirtFS>(filter, upd) { IsUpsert = true };
                updates.Add(update);
            }

            Console.WriteLine($"[Loaded {nCount} Namespaces and {vCount} Volumes]");

            // Persist

            NeoVirtFSCol.BulkWrite(updates);

            return HaveRoot;
        }

        public static ObjectId EnsureVolumeSetUpProperly(IMongoDatabase db, string volumeNamePath)
        {
            var HaveRoot = false;

            var NamespaceNames = new Dictionary<string, NeoVirtFSNamespaces>();
            var Namespaces = new Dictionary<ObjectId, NeoVirtFSNamespaces>();
            NeoVirtFSNamespaces RootNameSpace = null;

            var NeoVirtFSCol = db.NeoVirtFS();
            var NeoVirtFSDeletedCol = db.NeoVirtFSDeleted();
            var NeoVirtFSNamespacesCol = db.NeoVirtFSNamespaces();
            var NeoVirtFSVolumesCol = db.NeoVirtFSVolumes();
            var NeoVirtFSSecPrincipalsCol = db.NeoVirtFSSecPrincipals();
            var NeoVirtFSSecACLsCol = db.NeoVirtFSSecACLs();

            // Prepare for bulk update

            var updates = new List<WriteModel<NeoVirtFS>>();

            // Load up the namespaces -- there just shouldn't be too many of these

            ProcessNamespaces(db, NamespaceNames, Namespaces, ref RootNameSpace, ref HaveRoot, updates);
            if (!HaveRoot)
                throw new ApplicationException("Volume Namespaces don't define a Root - Setup Issue");

            // Now do volumes 

            var (parentPath, VolumeName) = FindVolumeNameWithPath(volumeNamePath, Namespaces, RootNameSpace);
            if (parentPath == null)  // We won't really get here - the above will throw
                throw new ArgumentException($"Volume Path Not Valid: {volumeNamePath}");

            // So we actually need to create this volume into NeoVirtFSVolumes, which unfortunately needs business logic
            // We need some sort of policy for this.  Maybe we're overthinking.  Maybe we just need to be passed the path so we
            //  don't need to decide?   We have the Namespaces above so we can decode

            var volume = NeoVirtFSVolumesCol.FindSync(Builders<NeoVirtFSVolumes>.Filter.Eq(x => x.Name, VolumeName)).FirstOrDefault();
            if (volume == null)
            {
                Console.WriteLine($"[Create Volume {VolumeName} into Namespace {parentPath.NameSpace}]");

                var newNode = ObjectId.GenerateNewId();

                // If the namespace moved then this isn't going to change it, though we may assert below

                NeoVirtFSVolumesCol.InsertOne(new NeoVirtFSVolumes()
                {
                    _id = newNode,
                    Name = VolumeName,
                    NameSpace = parentPath._id,
                    NodeId = newNode,  // These are intentionally the same
                    VolumeDefaultACL = null,
                    ImportLocked = false,
                    VolumeLocked = false,
                });
            }

            // This is only going to return our one volume (which we may have just created)
            var volFilter = Builders<NeoVirtFSVolumes>.Filter.Eq(x => x.Name, VolumeName);
            var v = NeoVirtFSVolumesCol.FindSync(volFilter).FirstOrDefault();

            // This is making sure that the NeoVirtFS elements match the Volume setup (they use the same keys for clarity)

            Console.WriteLine($"Volume: {v.Name}, NameSpace: {Namespaces[v.NameSpace].NameSpace}");

            // Assert if the namespace has moved - We can just move it

            if (v.NameSpace != parentPath._id)
            {
                Console.WriteLine($"Volume Namespace {Namespaces[v.NameSpace].NameSpace} is not same as current Namespace {Namespaces[parentPath._id].NameSpace}");

                v.NameSpace = parentPath._id;   // Just move the node to the correct place within namespaces

                // And do it in the table (not bulk)
                var fixPar = new UpdateDefinitionBuilder<NeoVirtFSVolumes>()
                    .Set("NameSpace", parentPath._id);

                NeoVirtFSVolumesCol.UpdateOne(volFilter, fixPar);
            }

            // Ensure that the filesystem nodes exist at the top level

            FilterDefinition<NeoVirtFS> filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, v._id);

            var upd = new UpdateDefinitionBuilder<NeoAssets.Mongo.NeoVirtFS>()
                .Set("_id", v._id)
                .SetOnInsert("Content", NeoVirtFSContent.Dir())
                .SetOnInsert("Stat", NeoVirtFSStat.DirDefault())
                .Set("VolumeId", v.NameSpace)
                .Set("ParentId", Namespaces[v.NameSpace]._id) // Set's see what root does - this will also move the Node to match the Volume
                .Set("Name", Encoding.UTF8.GetBytes(v.Name))
                .Set("MaintLevel", false);    // This is the volume level - users can do stuff here (by their policy)

            UpdateOneModel<NeoVirtFS> update = new UpdateOneModel<NeoAssets.Mongo.NeoVirtFS>(filter, upd) { IsUpsert = true };
            updates.Add(update);

            // Persist

            NeoVirtFSCol.BulkWrite(updates);

            return v._id;
        }

        /// <summary>
        /// Walk the path through the namespaces and see if this is a valid path.  Really we're finding
        /// the parent for the volume.  Someday this needs to be more dynamic in case we move the volume's rooting.
        /// </summary>
        /// <param name="volumeNamePath"></param>
        /// <param name="namespaces"></param>
        /// <param name="rootNameSpace"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static (NeoVirtFSNamespaces parentPath, string volumeName) FindVolumeNameWithPath(string volumeNamePath,
            Dictionary<ObjectId, NeoVirtFSNamespaces> namespaces,
            NeoVirtFSNamespaces rootNameSpace)
        {
            NeoVirtFSNamespaces retParent = null;

            // Build a quick index to traverse namespaces in a tree by name - ultimately we need to cache this

            var nameTree = new Dictionary<ObjectId, Dictionary<string, ObjectId>>();

            Console.WriteLine("Build Tree Index");
            foreach (var n in namespaces)
            {
                var p = n.Value.ParentId;

                if (nameTree.ContainsKey(p))
                    nameTree[p].Add(n.Value.NameSpace, n.Key);
                else
                {
                    var newT = new Dictionary<string, ObjectId>();
                    newT.Add(n.Value.NameSpace, n.Key);
                    nameTree.Add(n.Value.ParentId, newT);
                };
            }

            var paths = volumeNamePath.Split('/');

            // Start at the top 

            var rootLevel = rootNameSpace._id;
            var levelMembers = nameTree[rootLevel];

            //var levelStack = new List<NeoVirtFSNamespaces>();

            // Have top stop one before the end
            foreach (var p in paths[..^1])
            {
                if (levelMembers == null)
                    throw new ArgumentException($"Path Element {p} has no children");

                if (!levelMembers.ContainsKey(p))
                    throw new ArgumentException($"Path Element {p} is not found in namespaces: {volumeNamePath}");

                var mem = levelMembers[p];
                var level = namespaces[mem];
                //levelStack.Add(level);

                if (nameTree.ContainsKey(level._id))
                    levelMembers = nameTree[level._id];
                else
                    levelMembers = null;

                retParent = level;
            }

            // And return our tuple

            return (retParent, paths.Last());
        }

        private static int ProcessNamespaces(IMongoDatabase db,
            Dictionary<string, NeoVirtFSNamespaces> NamespaceNames,
            Dictionary<ObjectId, NeoVirtFSNamespaces> Namespaces,
            ref NeoVirtFSNamespaces RootNameSpace,
            ref bool HaveRoot,
            List<WriteModel<NeoVirtFS>> updates)
        {
            var NeoVirtFSNamespacesCol = db.NeoVirtFSNamespaces();
            var names = NeoVirtFSNamespacesCol.FindSync(Builders<NeoVirtFSNamespaces>.Filter.Empty).ToList();

            var nCount = 0;
            foreach (var n in names)
            {
                NamespaceNames[n.NameSpace] = n;
                Namespaces[n._id] = n;

                //Console.WriteLine($"Namespace: {n.NameSpace}");
                nCount++;

                if (n.Root)
                {
                    RootNameSpace = n;
                    HaveRoot = true;
                    n.ParentId = ObjectId.Empty;  // Until I figure out how to set this
                }

                // Ensure that the filesystem nodes exist at the top level

                FilterDefinition<NeoVirtFS> filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, n._id);

                var upd = new UpdateDefinitionBuilder<NeoAssets.Mongo.NeoVirtFS>()
                    .Set("_id", n._id)
                    .SetOnInsert("Content", NeoVirtFSContent.Dir())
                    .SetOnInsert("Stat", NeoVirtFSStat.DirDefault())
                    .Set("VolumeId", n._id)
                    .Set("ParentId", n.ParentId) // Set's see what root does
                    .Set("Name", Encoding.UTF8.GetBytes(n.NameSpace))
                    .Set("MaintLevel", true);

                UpdateOneModel<NeoVirtFS> update = new UpdateOneModel<NeoAssets.Mongo.NeoVirtFS>(filter, upd) { IsUpsert = true };
                updates.Add(update);
            }

            return nCount;
        }

        public byte[] GetStatPhysical()
        {
            switch (Content.ContentType)
            {
                case VirtFSContentTypes.NotAFile:
                    return null;
                case VirtFSContentTypes.Asset:
                    return null;
                case VirtFSContentTypes.MountedVolume:
                    return null;
                case VirtFSContentTypes.PhysicalFile:
                    return Content.PhysicalFile;
                case VirtFSContentTypes.CachePool:
                    return Content.CacheFile;
            }

            return null;
        }

        public bool DeepAnneal(IMongoDatabase db, string checkFile, bool assetDebug, out byte[] sha1)
        {
            sha1 = Array.Empty<byte>();

            switch (Content.ContentType)
            {
                case VirtFSContentTypes.NotAFile:
                    return false;
                case VirtFSContentTypes.Asset:
                    var bad = new byte[] { (byte) '.', (byte) 'b', (byte) 'a', (byte) 'd' };
                    if (checkFile != null)
                        RenameFile(checkFile, bad);
                    return false;
                case VirtFSContentTypes.MountedVolume:
                    return false;
                case VirtFSContentTypes.PhysicalFile:
                    return DeepAnnealFile(db, Content.PhysicalFile, null, assetDebug, out sha1);
                case VirtFSContentTypes.CachePool:
                    var cFile = checkFile;
                    if (Content.CacheFile.SequenceEqual(Encoding.ASCII.GetBytes(cFile)))
                        cFile = null;
                    return DeepAnnealFile(db, Content.CacheFile, cFile, assetDebug, out sha1);
            }

            return false;
        }

        private unsafe bool DeepAnnealFile(IMongoDatabase db, byte[] fileName, string realCache, bool assetDebug, out byte[] catchSha1)
        {
            // We need to get the SHA1 hash

            catchSha1 = Array.Empty<byte>();

            var hash = fastSha1(fileName);
            if (hash == null) return false;

            catchSha1 = hash;

            var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
            Console.WriteLine($"[Anneal: SHA1 {hashHex}]");

            // So, we see if we've got an annealed asset

            var bac = db.BakedAssets();
            var bav = db.BakedVolumes();
            var nvb = db.NeoVirtFS();

            var theFilter = Builders<BakedAssets>.Filter.Eq("_id", hashHex);
            var baRec = bac.FindSync(theFilter).FirstOrDefault();
            if (baRec == null)
            {
                Console.WriteLine($"Can't find an asset for {hashHex} file {fileName.GetString()}");
                return false;
            }

            if (!baRec.Annealed.Value)
            {
                Console.WriteLine($"This asset is not annealed {hashHex} file {fileName.GetString()}");
                return false;
            }

            // Ok.  Now we've got an asset.   We're going to see if it's actually valid 
            // (Which is the "Deep" part).   We're not going to assume for now until this is truly all tested.

            var catchHash = GetAssetHash(db, hashHex, bac, bav, assetDebug);

            // If the hash is the same then we can anneal
            if (hashHex == catchHash)
            {
                var purgeFile = (Content.ContentType == VirtFSContentTypes.CachePool);

                Content = NeoVirtFSContent.AnnealedAsset(hash, false);

                var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, this._id);
                var insert = nvb.ReplaceOne(filter, this, options: new ReplaceOptions { IsUpsert = false });
                Console.WriteLine($"[Deep Anneal {fileName.GetString()} Id {_id} SHA1 {hashHex}]");

                // Though if the insert succeded we really need to delete the old file (or rename) - only if this was a cache

                if (purgeFile)
                {
                    var del = new byte[] { (byte) '.', (byte) 'd', (byte) 'e', (byte) 'l', (byte) 'e', (byte) 't', (byte) 'e', (byte) 'd' };

                    RenameFile(fileName, del);
                }

                // Rename the incoming file if it's not the same cachefile as the object has (we already just renamed that)
                var bad = new byte[] { (byte) '.', (byte) 'b', (byte) 'a', (byte) 'd' };
                if (realCache != null)
                    RenameFile(realCache, bad);

                return true;
            }
            else
                Console.WriteLine($"Anneal: The Baked Hash should have been {hashHex} but was {catchHash}");


            return false;
        }
        private static void RenameFile(string fileName, byte[] del)
        {
            RenameFile(Encoding.ASCII.GetBytes(fileName), del);
        }
        private unsafe static void RenameFile(byte[] fileName, byte[] del)
        {
            var newPath = new List<byte>(fileName);
            newPath.AddRange(del);

            var res = LibC.unlink(toBp(newPath.ToArray()));
            // Ignore result - file probably doesn't exist

            res = LibC.link(toBp(fileName), toBp(newPath.ToArray()));
            if (res < 0)
                Console.WriteLine($"Could not rename away {LibC.errno}");

            res = LibC.unlink(toBp(fileName));
            // We will ignore this too
        }

        private static string GetAssetHash(IMongoDatabase db, string hashHex, IMongoCollection<BakedAssets> bac, IMongoCollection<BakedVolumes> bav, bool assetDebug)
        {
            Console.WriteLine($"Check Baked File: {hashHex}");

            var assetLink = new AssetFileSystem.AssetFile.UnbakeForFuse(db, bac, bav, hashHex, assetDebug);

            string catchHash = null;
            using (var hasher = System.Security.Cryptography.SHA1.Create())
            {
                var buffer = new Span<byte>(new byte[4096]);
                var r = buffer.Length;

                ulong offset = 0;

                try
                {

                    while (r > 0)
                    {
                        r = assetLink.Read(offset, buffer);
                        if (r > 0)
                        {
                            offset += (ulong) r;
                            var bf = buffer.ToArray();
                            hasher.TransformBlock(bf, 0, (int) r, bf, 0);
                        }
                    }

                    hasher.TransformFinalBlock(buffer.ToArray(), 0, 0);

                    catchHash = Convert.ToHexString(hasher.Hash).ToLowerInvariant();
                }               
                catch (ZlibException ex) {
                    Console.WriteLine($"UnbakeForFuse::GetAssetHash {ex.Message}");
                    return null;
                }
            }

            return catchHash;
        }

        private static unsafe byte[] fastSha1(byte[] fileName)
        {
            var iot = LibC.open(toBp(fileName), 0);
            if (iot < 0)
            {
                Console.WriteLine($"Can't open file: {fileName.GetString()}");
                return null;
            }

            using (var hasher = System.Security.Cryptography.SHA1.Create())
            {
                var buffer = new Span<byte>(new byte[4096]);
                ssize_t r = buffer.Length;

                while (r > 0)
                {
                    fixed (byte* b = buffer)
                    {
                        r = LibC.read(iot, b, buffer.Length);
                    }

                    if (r > 0)
                    {
                        var bf = buffer.ToArray();
                        hasher.TransformBlock(bf, 0, (int) r, bf, 0);
                    }
                }

                hasher.TransformFinalBlock(buffer.ToArray(), 0, 0);

                return hasher.Hash;
            }
        }

        public static unsafe byte* toBp(ReadOnlySpan<byte> path)
        {
            return RawDirs.ToBytePtr(path.ToArray());
        }
        public static unsafe byte* toBp(byte[] path)
        {
            return RawDirs.ToBytePtr(path);
        }
        public NeoVirtFS MakeLink(NeoVirtFS par, byte[] newFile)
        {
            var ret = (NeoVirtFS) this.MemberwiseClone();

            // New Record
            ret._id = ObjectId.GenerateNewId();

            // Reparent
            ret.VolumeId = par.VolumeId;
            ret.ParentId = par._id;

            // New name
            ret.Name = newFile;

            return ret;
        }

        public List<Byte[]> GetAttributes()
        {
            var ret = new List<Byte[]>();

            // Return fixed attributes
            ret.Add(Encoding.ASCII.GetBytes("_id"));
            ret.Add(Encoding.ASCII.GetBytes("Version"));
            ret.Add(Encoding.ASCII.GetBytes("VersionId"));

            // Return content based attributes
            Content.GetAttributeList(ret);

            // Return dynamic attributes
            foreach (var a in Attributes.Elements)
                ret.Add(Encoding.ASCII.GetBytes(a.Name));

            return ret;
        }

        public void Rename(NeoVirtFS par, byte[] newFile)
        {
            Console.WriteLine($"Rename to {newFile.GetString()} on Parent {par.Name.GetString()}");

            // We're getting attached to par and renamed

            Name = newFile;

            // Pull in parent stuff

            VolumeId = par.VolumeId;
            ParentId = par._id;

        }

        public unsafe int Truncate(ulong length)
        {
            switch (Content.ContentType)
            {
                case VirtFSContentTypes.CachePool:

                    var res = LibC.truncate(RawDirs.ToBytePtr(Content.CacheFile), (long) length);

                    if (res < 0) res = -LibC.errno;
                    return res;
            }

            return -LibC.EPERM;
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
        [BsonIgnoreIfNull] public bool AssetLost { get; set; }          // Asset volume was lost (disk failure) - this is so data can be ressurected

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

        public static NeoVirtFSContent AnnealedAsset(byte[] AssetSHA1, bool lost)
        {
            var ret = new NeoVirtFSContent
            {
                ContentType = VirtFSContentTypes.Asset,
                NotAFile = false,
                AssetSHA1 = AssetSHA1,
                AssetLost = lost
            };

            return ret;
        }

        public static NeoVirtFSContent PhysicalFilePath(byte[] filePath)
        {
            var ret = new NeoVirtFSContent
            {
                ContentType = VirtFSContentTypes.PhysicalFile,
                NotAFile = false,
                PhysicalFile = filePath
            };

            return ret;
        }

        internal static NeoVirtFSContent NewCache(ReadOnlySpan<byte> path, ObjectId nodeObj)
        {
            var fileuuid = GuidUtility.Create(GuidUtility.UrlNamespace, path.ToArray());

            var fileg = fileuuid.ToString().ToLower();

            // 8478e36a-669c-11ec-8541-17df4f57891e
            // 0123456789012345678901234567890123456789
            // 000000000011111111112222222222333333333

            // Ultimately will need real pool mechanism here

            // Original scheme was absurd - the 3 digit is 4096.  If we're sufficiently event driven and reactive then this shouldn't get
            // out of hand -- it should handle millions of physical cache files without too much difficulty, and really if they're annealled
            // quickly enough there should never be that many on a given filesystem

            //var cacheFile = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}/{fileg.Substring(2, 2)}/{fileg.Substring(4, 2)}/{fileg.Substring(6, 2)}/{fileg}_{nodeObj}";
            var cacheFile = $"/ua/NeoVirtCache/{fileg.Substring(0, 3)}/{fileg}_{nodeObj}";

            var rec = new NeoVirtFSContent
            {
                CachePoolGuid = fileuuid,
                ContentType = VirtFSContentTypes.CachePool,
                CacheFile = Encoding.UTF8.GetBytes(cacheFile)
            };

            return rec;
        }

        internal void GetAttributeList(List<byte[]> ret)
        {
            switch (ContentType)
            {
                case VirtFSContentTypes.NotAFile:
                    break;
                case VirtFSContentTypes.Asset:  // Annealed files
                    exposeAsset(AssetSHA1, ret);
                    // If we load the asset, we might expose many of the asset attributes, at least hashes and file types
                    break;

                case VirtFSContentTypes.MountedVolume:

                // The following also might conceivably have a hash (not annealed yet) - expose Asset attributes
                case VirtFSContentTypes.CachePool:
                case VirtFSContentTypes.PhysicalFile:
                    break;
            }
        }
        private void exposeAsset(byte[] assetSHA1, List<byte[]> ret)
        {
            ret.Add(Encoding.ASCII.GetBytes("HASH.SHA1"));
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

        public NeoVirtFSStat(AssetFiles_Stat stat)
        {
            st_size = Convert.ToUInt64(stat.size);

            st_uid = stat.uid;
            st_gid = stat.gid;

            st_mode = (NeoMode_T) Convert.ToUInt32(stat.mode);

            st_ctim = DateTimeOffset.FromUnixTimeMilliseconds(stat.ctime * 1000);
            st_mtim = DateTimeOffset.FromUnixTimeMilliseconds(stat.mtime * 1000);
            st_atim = DateTimeOffset.FromUnixTimeMilliseconds(stat.atime * 1000);

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

        // Expose possible captured hashes - invalidate on write
        [BsonIgnoreIfNull] public Dictionary<string, Byte[]> Hashes { get; set; }

        public bool IsDirectory { get { return (st_mode & NeoMode_T.S_IFDIR) != 0; } }
        public bool IsFile { get { return (st_mode & NeoMode_T.S_IFREG) != 0; } }

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

        // Cast us to a file type 
        public NeoVirtFSStat ToFile()
        {
            var newFile = (NeoVirtFSStat) this.MemberwiseClone();

            newFile.st_mode = NeoMode_T.S_IFREG | (st_mode & (~NeoMode_T.S_IFMT));

            return newFile;
        }

        internal void GetStat(ref stat stat)
        {
            stat.st_nlink = 1;

            stat.st_ino = 1;

            stat.st_size = Convert.ToInt64(st_size);
            stat.st_blksize = 4096;

            stat.st_blocks = (blkcnt_t) (long) (stat.st_size >> 9);
            if (st_size % 512 > 0) stat.st_blocks += 1;

            stat.st_uid = st_uid;
            stat.st_gid = st_gid;

            stat.st_mode = st_mode.GetMode();

            stat.st_atim = st_atim.GetTimeSpec();
            stat.st_ctim = st_ctim.GetTimeSpec();
            stat.st_mtim = st_mtim.GetTimeSpec();
        }

        internal void GetStat(ref stat stat, stat inStats)
        {
            stat.st_nlink = 1;

            stat.st_ino = 1;

            stat.st_size = Convert.ToInt64(inStats.st_size);
            stat.st_blksize = 4096;

            stat.st_blocks = (blkcnt_t) (long) (stat.st_size >> 9);
            if (st_size % 512 > 0) stat.st_blocks += 1;

            stat.st_uid = st_uid;
            stat.st_gid = st_gid;

            stat.st_mode = st_mode.GetMode();

            stat.st_atim = st_atim.GetTimeSpec();
            stat.st_ctim = st_ctim.GetTimeSpec();
            stat.st_mtim = st_mtim.GetTimeSpec();
        }

        public static NeoVirtFSStat FileDefault(uint mode = 0b110_100_100)
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
        [BsonIgnoreIfNull] public bool VolumeLocked { get; set; }
        [BsonIgnoreIfNull] public bool ImportLocked { get; set; }

        public static void EnsureNarpVolumeExists(IMongoDatabase db, string narp)
        {
            // Make sure this is really a narp
            var nRec = NARPs.GetNARP(db, narp);
            if (nRec == null)
                throw new ArgumentException($"NARP {narp} doesn't exist");

            Console.WriteLine($"[NARP {narp} Exists]");
        }
    }

    public enum NeoVirtFSSecPrincipalTypes
    {
        Owner = 1,
        Role = 2,
    }
    public class NeoVirtFSSecPrincipals
    {
        [BsonId] public ObjectId _id { get; set; }
        [BsonRequired] public NeoVirtFSSecPrincipalTypes PrincipalType { get; set; }
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
