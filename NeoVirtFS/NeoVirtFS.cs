using MongoDB.Bson;
using MongoDB.Driver;
using NeoAssets.Mongo;
using PenguinSanitizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.Fuse;
using Tmds.Linux;
using static PenguinSanitizer.Extensions;

namespace NeoVirtFS
{
    // mounts to /neo/<namespace>/<volumename>
    // Volumename points to a directory in the virtual system to denote the root dir

    public class NeoVirtFS : FuseFileSystemBase
    {
        #region Class Persistence
        IMongoCollection<NeoAssets.Mongo.NeoVirtFS> NeoVirtFSCol;
        IMongoCollection<NeoVirtFSNamespaces> NeoVirtFSNamespacesCol;
        IMongoCollection<NeoVirtFSVolumes> NeoVirtFSVolumesCol;
        IMongoCollection<NeoVirtFSSecPrincipals> NeoVirtFSSecPrincipalsCol;
        IMongoCollection<NeoVirtFSSecACL> NeoVirtFSSecACLsCol;

        Dictionary<string, NeoVirtFSNamespaces> NamespaceNames = new Dictionary<string, NeoVirtFSNamespaces>();
        Dictionary<ObjectId, NeoVirtFSNamespaces> Namespaces = new Dictionary<ObjectId, NeoVirtFSNamespaces>();

        int verbosity = 10;

        NeoVirtFSNamespaces RootNameSpace = null;
        #endregion

        #region Constructor
        public NeoVirtFS(IMongoDatabase db)
        {
            // Get wired up to our collections
            NeoVirtFSCol = db.NeoVirtFS();
            NeoVirtFSNamespacesCol = db.NeoVirtFSNamespaces();
            NeoVirtFSVolumesCol = db.NeoVirtFSVolumes();
            NeoVirtFSSecPrincipalsCol = db.NeoVirtFSSecPrincipals();
            NeoVirtFSSecACLsCol = db.NeoVirtFSSecACLs();

            bool HaveRoot = false;

            // Prepare for bulk update

            var updates = new List<WriteModel<NeoAssets.Mongo.NeoVirtFS>>();

            // Load up the namespaces -- there just shouldn't be too many of these

            var names = NeoVirtFSNamespacesCol.FindSync(Builders<NeoVirtFSNamespaces>.Filter.Empty).ToList();
            foreach (var n in names)
            {
                NamespaceNames[n.NameSpace] = n;
                Namespaces[n._id] = n;

                if (verbosity > 0)
                    Console.WriteLine($"Namespace: {n.NameSpace}");

                if (n.Root)
                {
                    RootNameSpace = n;
                    HaveRoot = true;
                    n.ParentId = ObjectId.Empty;  // Until I figure out how to set this
                }

                // Ensure that the filesystem nodes exist at the top level

                FilterDefinition<NeoAssets.Mongo.NeoVirtFS> filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, n._id);

                var upd = new UpdateDefinitionBuilder<NeoAssets.Mongo.NeoVirtFS>()
                    .Set("_id", n._id)
                    .SetOnInsert("Content", NeoVirtFSContent.Dir())
                    .SetOnInsert("Stat", NeoVirtFSStat.DirDefault())
                    .Set("NameSpace", n._id)
                    .Set("ParentId", n.ParentId) // Set's see what root does
                    .Set("Name", Encoding.UTF8.GetBytes(n.NameSpace))
                    .Set("MaintLevel", true);

                UpdateOneModel<NeoAssets.Mongo.NeoVirtFS> update = new UpdateOneModel<NeoAssets.Mongo.NeoVirtFS>(filter, upd) { IsUpsert = true };
                updates.Add(update);
            }



            // Now do volumes

            var volumes = NeoVirtFSVolumesCol.FindSync(Builders<NeoVirtFSVolumes>.Filter.Empty).ToList();
            foreach (var v in volumes)
            {
                if (verbosity > 0)
                    Console.WriteLine($"Volume: {v.Name}");

                // Ensure that the filesystem nodes exist at the top level

                FilterDefinition<NeoAssets.Mongo.NeoVirtFS> filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, v._id);

                var upd = new UpdateDefinitionBuilder<NeoAssets.Mongo.NeoVirtFS>()
                    .Set("_id", v._id)
                    .SetOnInsert("Content", NeoVirtFSContent.Dir())
                    .SetOnInsert("Stat", NeoVirtFSStat.DirDefault())
                    .Set("NameSpace", v.NameSpace)
                    .Set("ParentId", Namespaces[v.NameSpace]._id) // Set's see what root does
                    .Set("Name", Encoding.UTF8.GetBytes(v.Name))
                    .Set("MaintLevel", false);    // This is the volume level - users can do stuff here (by their policy)

                UpdateOneModel<NeoAssets.Mongo.NeoVirtFS> update = new UpdateOneModel<NeoAssets.Mongo.NeoVirtFS>(filter, upd) { IsUpsert = true };
                updates.Add(update);
            }

            // Persist

            NeoVirtFSCol.BulkWrite(updates);

            if (!HaveRoot)
                throw new Exception("Did not define a root namespace");
        }
        #endregion


        #region Fuse Methods
        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            if (verbosity > 0)
                Console.WriteLine($"GetAttr {path.GetString()}");



            int error = 0, level=0;
            var procs = ProcPath(path, ref error, ref level);


            var last = procs.Pop();
            Console.WriteLine($"   getAttr - last {last.Item1.GetString()} error {last.Item3}");

            if (last.Item2 == null)
                return -last.Item3;

            Console.WriteLine($" return: {last.Item2.Stat.st_mode.ModMask()}");

            last.Item2.GetStat(ref stat);
            return 0;
        }
        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"OpenDir {path.GetString()} - NoOp");

            return 0;

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.OpenDir(path, ref fi);
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, isADir: true);

            var last = procs.Pop();
       

            if (last.Item2 == null) return -last.Item3;  // No such file



            // Just return everything - someday we need to break into batches

            content.AddEntry(".");
            content.AddEntry("..");

            if (verbosity > 0)
                Console.WriteLine($"ReadDir Path={path.GetString()} offset={offset} flags={flags} ParentId={last.Item2._id}");

            // This could conceivably return millions of records - Once we figure out how to set up batches
            // we can use OpenDir to create a file handle which we can bind an enumerator to (RelaseDir can close it).

            // ParentId should be indexed (with a hash)
            var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, last.Item2._id);
            foreach (var rec in NeoVirtFSCol.FindSync(filter).ToList())
            {
                Console.WriteLine($"  Contents: {rec._id} - {rec.Name.GetString()}");
                content.AddEntry(rec.Name);
            }

            return 0;

        }
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"ReleaseDir {path.GetString()} - NoOp");

            return 0; 

            int error = 0, level=0;
            var procs = ProcPath(path, ref error,ref level, isADir: true);

            return base.ReleaseDir(path, ref fi);
        }

        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            if (verbosity > 0)
                Console.WriteLine($"MkDir {path.GetString()} mode={mode}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist: false, isADir: true);

            if (procs.Count < 2)
            {
                Console.WriteLine($"Mkdir - path < 2");
                return -LibC.EPERM;
            }

            var cNode = procs.Pop();
            var inNode = procs.Pop();

            if (cNode.Item2 != null)
                return -LibC.EEXIST;  // File (or directory) already exists

            if (inNode.Item2 == null)
                return -LibC.ENOENT;   // SHouldn't happen (2nd level will get error)

            var createIn = inNode.Item2;

            if (createIn.MaintLevel)
                return -LibC.EPERM;     // Nobody can create in the maintenance levels (Namespace levels)

            var newRec = new NeoAssets.Mongo.NeoVirtFS()
            {
                _id = new ObjectId(),
                Content = NeoVirtFSContent.Dir(),
                Stat = NeoVirtFSStat.DirDefault((uint) mode),
                Name = cNode.Item1,
                NameSpace = createIn.NameSpace,
                ParentId = createIn._id,
                MaintLevel = false,
            };  // It will assign it's own objectId _id

            NeoVirtFSCol.InsertOne(newRec);
            Console.WriteLine($"Create Directory {cNode.Item1.GetString()} id={newRec._id}");

            return 0;
        }

        public override int Access(ReadOnlySpan<byte> path, mode_t mode)
        {
            if (verbosity > 0)
                Console.WriteLine($"Access {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Access(path, mode);
        }
        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            if (verbosity > 0)
                Console.WriteLine($"ChMod {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.ChMod(path, mode, fiRef);
        }
        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            if (verbosity > 0)
                Console.WriteLine($"Chown {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Chown(path, uid, gid, fiRef);
        }
        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"Create {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Create(path, mode, ref fi);
        }
        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"FAllocate {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.FAllocate(path, mode, offset, length, ref fi);
        }
        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"Flush {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Flush(path, ref fi);
        }
        public override int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"FSync {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.FSync(path, ref fi);
        }


        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
        {
            if (verbosity > 0)
                Console.WriteLine($"Link {fromPath.GetString()} {toPath.GetString()} ");

            int error = 0, level = 0;
            var procs = ProcPath(fromPath, ref error, ref level);

            return base.Link(fromPath, toPath);
        }
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            if (verbosity > 0)
                Console.WriteLine($"Unlink {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Unlink(path);
        }
        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target)
        {
            if (verbosity > 0)
                Console.WriteLine($"SymLink {path.GetString()} {target.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.SymLink(path, target);
        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer)
        {
            if (verbosity > 0)
                Console.WriteLine($"ReadLink {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.ReadLink(path, buffer);
        }



        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"Open {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Open(path, ref fi);
        }
        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"Read {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Read(path, offset, buffer, ref fi);
        }
        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"Write {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Write(path, off, span, ref fi);
        }
        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            if (verbosity > 0)
                Console.WriteLine($"Truncate {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Truncate(path, length, fiRef);
        }
        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"Release {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            base.Release(path, ref fi);
        }


      
        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            if (verbosity > 0)
                Console.WriteLine($"Rename {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.Rename(path, newPath, flags);
        }

        public override int RmDir(ReadOnlySpan<byte> path)
        {
            if (verbosity > 0)
                Console.WriteLine($"RmDir {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, isADir: true);

            return base.RmDir(path);
        }
        public override int FSyncDir(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"FSyncDir {path.GetString()}");

            return base.FSyncDir(path, onlyData, ref fi);
        }

        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            if (verbosity > 0)
                Console.WriteLine($"ListXAttr {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.ListXAttr(path, list);
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            if (verbosity > 0)
                Console.WriteLine($"RemoveXAttr {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.RemoveXAttr(path, name);
        }
        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            if (verbosity > 0)
                Console.WriteLine($"GetXAttr {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.GetXAttr(path, name, data);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            if (verbosity > 0)
                Console.WriteLine($"SetXAttr {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.SetXAttr(path, name, data, flags);
        }


        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            if (verbosity > 0)
                Console.WriteLine($"StatFS {path.GetString()}");

            int error = 0,level=0;
            var procs = ProcPath(path, ref error, ref level, isADir: true);

            return base.StatFS(path, ref statfs);
        }



        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            if (verbosity > 0)
                Console.WriteLine($"UpdateTimestamps {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);

            return base.UpdateTimestamps(path, ref atime, ref mtime, fiRef);
        }

        #endregion

        #region Helper Methods
        private Stack<Tuple<byte[], NeoAssets.Mongo.NeoVirtFS?, int>> ProcPath(ReadOnlySpan<byte> path, ref int error, ref int outLevel, bool mustExist = true, bool isADir = false)
        {
            error = 0;

            ObjectId retId = ObjectId.Empty;

            var stack = new Stack<Tuple<byte[], NeoAssets.Mongo.NeoVirtFS?, int>>();

            // Split on / - Has to be a way to do this with slices

            var ret = new List<byte[]>();
            var buf = new List<byte>();

            foreach (var c in path)
            {
                if (c == '/')
                {
                    if (buf.Count > 0)
                    {
                        ret.Add(buf.ToArray());
                        buf = new List<byte>();
                    }
                }
                else
                    buf.Add(c);
            }

            if (buf.Count > 0)
            {
                ret.Add(buf.ToArray());
                buf = new List<byte>();
            }

            // This needs a cache - someday - maybe by open or opendir path

            var retA = ret.ToArray();
            Console.WriteLine($"Path: {path.GetString()} {retA.Length}");
            for (var i = 0; i < retA.Length; i++)
                Console.WriteLine($" {i}: {retA[i].GetString()}");

            // Get the Root level
            retId = RootNameSpace._id;

            Console.WriteLine($"  - Get Root {retId}");
            var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, retId);
            var node = NeoVirtFSCol.FindSync(filter).FirstOrDefault();

            Console.WriteLine($"Level: {retA.Length}");

            error = node == null ? LibC.ENOENT : 0;

            stack.Push(new Tuple<byte[], NeoAssets.Mongo.NeoVirtFS?, int>(Array.Empty<byte>(), node, error));

            outLevel = retA.Length;

            if (retA.Length < 1 || node == null)
                return stack;

            // Now we walk down

            foreach (var level in retA)
            {
                Console.WriteLine($"  - Get {level.GetString()} Parent={node._id}");
                filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, node._id) &
                     Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.Name, level);

                node = NeoVirtFSCol.FindSync(filter).FirstOrDefault();
                error = node == null ? LibC.ENOENT : 0;

                stack.Push(new Tuple<byte[], NeoAssets.Mongo.NeoVirtFS?, int>(level, node, error));

                if (node == null)
                    return stack;

                // We need a better error if we can't go all the way to the end
            }

            Console.WriteLine($"Found: {node.Name.GetString()} Id {node._id}");

            return stack;
        }


        #endregion
    }
}
