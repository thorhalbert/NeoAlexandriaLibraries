using MongoDB.Driver;
using NeoAssets.Mongo;
using NeoCommon;
using PenguinSanitizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.Fuse;
using Tmds.Linux;
using static PenguinSanitizer.Extensions;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using NeoBakedVolumes.Mongo;

namespace NeoVirtFS
{
    // mounts to /neo/<namespace>/<volumename>
    // Volumename points to a directory in the virtual system to denote the root dir

    public class NeoVirtFS : FuseFileSystemBase
    {
        public IMongoDatabase db { get; }
        #region Class Persistence
        IMongoCollection<NeoAssets.Mongo.NeoVirtFS> NeoVirtFSCol;
        IMongoCollection<NeoAssets.Mongo.NeoVirtFS> NeoVirtFSDeletedCol;
        IMongoCollection<NeoVirtFSNamespaces> NeoVirtFSNamespacesCol;
        IMongoCollection<NeoVirtFSVolumes> NeoVirtFSVolumesCol;
        IMongoCollection<NeoVirtFSSecPrincipals> NeoVirtFSSecPrincipalsCol;
        IMongoCollection<NeoVirtFSSecACL> NeoVirtFSSecACLsCol;

        IMongoCollection<NeoBakedVolumes.Mongo.BakedAssets> bac;
        IMongoCollection<NeoBakedVolumes.Mongo.BakedVolumes> bvol;

        Dictionary<string, NeoVirtFSNamespaces> NamespaceNames = new Dictionary<string, NeoVirtFSNamespaces>();
        Dictionary<ObjectId, NeoVirtFSNamespaces> Namespaces = new Dictionary<ObjectId, NeoVirtFSNamespaces>();

        ulong FileDescriptorMax = 10;
        Dictionary<ulong, FileDescriptor> DescriptorStore = new Dictionary<ulong, FileDescriptor>();
        Dictionary<ulong, bool> DescriptorFree = new Dictionary<ulong, bool>();

        private static MemoryCache nodeCache=null;

        int verbosity = 1;

        NeoVirtFSNamespaces RootNameSpace = null;
        #endregion

        #region Constructor
        public NeoVirtFS(IMongoDatabase db)
        {
            this.db = db;
            // Get wired up to our collections
            NeoVirtFSCol = db.NeoVirtFS();
            NeoVirtFSDeletedCol = db.NeoVirtFSDeleted();
            NeoVirtFSNamespacesCol = db.NeoVirtFSNamespaces();
            NeoVirtFSVolumesCol = db.NeoVirtFSVolumes();
            NeoVirtFSSecPrincipalsCol = db.NeoVirtFSSecPrincipals();
            NeoVirtFSSecACLsCol = db.NeoVirtFSSecACLs();

            bac = db.BakedAssets();
            bvol = db.BakedVolumes();

            if (nodeCache == null)      // Some other instance might have done this
            {
                var oa = new MemoryCacheOptions()
                {
                    // SizeLimit = 100 * 1024 * 1024 
                };  // 100 mb?
                nodeCache = new MemoryCache(oa);
            }

            var HaveRoot = NeoAssets.Mongo.NeoVirtFS.PullNamespacesAndVolumes(db,
                ref NamespaceNames,
                ref Namespaces,
                ref RootNameSpace);

            if (!HaveRoot)
                throw new Exception("Did not define a root namespace");
        }

     
        #endregion


        #region Fuse Methods
        public unsafe override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            try
            {
                if (verbosity > 10)
                    Console.WriteLine($"GetAttr {path.GetString()}");

                int error = 0, level = 0;
                var procs = ProcPath(path, ref error, ref level, mustExist: true);
                if (error != 0)
                    return -LibC.ENOENT;

                var last = procs.Pop();
                //Console.WriteLine($"   getAttr - last {last.Item1.GetString()} error {last.Item3}");

                if (last.Item2 == null)
                    return -last.Item3;

                //Console.WriteLine($" return: {last.Item2.Stat.st_mode.ModMask()}");

                last.Item2.GetStat(ref stat);
                var physFile = last.Item2.GetStatPhysical();
                if (physFile != null)
                {
                    var nstat = new stat();

                    var lc = LibC.lstat(RawDirs.ToBytePtr(physFile.ToArray()), &nstat);
                    if (lc < 0)
                        return -LibC.errno;

                    // Real stat is kept in a physical backing file (mostly st_size)
                    // Not trying to keep this syned in the database yet
                    last.Item2.GetStat(ref stat, nstat);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error GetAttr: {ex.Message} {ex.StackTrace}");
                return -LibC.EIO;
            }
        }
        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 5)
                Console.WriteLine($"OpenDir {path.GetString()} - NoOp");

            return 0;

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.OpenDir(path, ref fi);
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, isADir: true);
            if (error != 0)
                return -LibC.ENOENT;


            var last = procs.Pop();
       

            if (last.Item2 == null) return -last.Item3;  // No such file



            // Just return everything - someday we need to break into batches

            content.AddEntry(".");
            content.AddEntry("..");

            if (verbosity > 5)
                Console.WriteLine($"ReadDir Path={path.GetString()} offset={offset} flags={flags} ParentId={last.Item2._id}");

            // This could conceivably return millions of records - Once we figure out how to set up batches
            // we can use OpenDir to create a file handle which we can bind an enumerator to (RelaseDir can close it).

            // ParentId should be indexed (with a hash)
            var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, last.Item2._id);
            foreach (var rec in NeoVirtFSCol.FindSync(filter).ToList())
            {
                //Console.WriteLine($"  Contents: {rec._id} - {rec.Name.GetString()}");
                content.AddEntry(rec.Name);

                // For now, also populate the cache with this

                NodeCacheSet(rec, 5);
            }

            return 0;

        }
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 5)
                Console.WriteLine($"ReleaseDir {path.GetString()} - NoOp");

            return 0; 

            int error = 0, level=0;
            var procs = ProcPath(path, ref error,ref level, isADir: true);
            if (error != 0)
                return -LibC.ENOENT;


            return base.ReleaseDir(path, ref fi);
        }

        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            if (verbosity > 5)
                Console.WriteLine($"MkDir {path.GetString()} mode={mode}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist: false, isADir: true);
            if (error != 0)
                return -LibC.ENOENT;


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
                _id = ObjectId.GenerateNewId(),
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

        public override int RmDir(ReadOnlySpan<byte> path)
        {
            if (verbosity > 5)
                Console.WriteLine($"RmDir {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist: true);
            if (error != 0)
                return -LibC.ENOENT;

            int version = 0;
            var newFile = procs.Pop();

            var parentRec = procs.Pop();
            if (parentRec.Item2 == null) return -LibC.ENOENT;
            var par = parentRec.Item2;

            if (par.MaintLevel) return -LibC.EPERM;

            if (newFile.Item2 != null)
            {
                // Any children?
                var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, newFile.Item2._id);
                var rec = NeoVirtFSCol.FindSync(filter).FirstOrDefault();  // Any record which returns is a no
                if (rec != null) return -LibC.ENOTEMPTY;   // Nope Nope Nope

                // Must delete the file
                version = newFile.Item2.Version;
                Console.WriteLine($"Directory Deleted: {newFile.Item2.Name.GetString()} id={newFile.Item2._id} Version={version}");

                fileDelete(newFile.Item2, DeleteTypes.RMDIR);
            }

            return 0;
        }

        public override int Access(ReadOnlySpan<byte> path, mode_t mode)
        {
            if (verbosity > 5)
                Console.WriteLine($"Access {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;

            var rec = procs.Pop();
            if (rec.Item2 == null) return LibC.ENOENT;

            //var res = LibC.access(toBp(path), (int) mode);
            //if (res < 0)
            //    return -LibC.errno;

            return 0;
        }
        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            if (verbosity > 5)
                Console.WriteLine($"ChMod {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;

            var ent = procs.Pop();
            if (ent.Item2 == null) return -LibC.ENOENT;

            var rec = ent.Item2;

            if (rec.MaintLevel) return -LibC.EPERM;

            var update = new UpdateDefinitionBuilder<NeoAssets.Mongo.NeoVirtFS>()
                  .Set(rec => rec.Stat.st_mode, (NeoMode_T) (uint) mode);

            var result = NeoVirtFSCol.UpdateOne(Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, rec._id), update);

            return 0;
        }
        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            if (verbosity > 5)
                Console.WriteLine($"Chown {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return 0;
        }
       
        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"FAllocate {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.FAllocate(path, mode, offset, length, ref fi);
        }
        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"Flush {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.Flush(path, ref fi);
        }
        public override int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 0)
                Console.WriteLine($"FSync {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return 0;
        }


        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
        {
            if (verbosity > 5)
                Console.WriteLine($"Link {fromPath.GetString()} {toPath.GetString()} ");

            // Link only creates links to files, not directories, so ultimate of fromPath must be a file
            // if ultimate of toPath is a directory then we create fromPath ultimate name in that directory
            // or we close fromPath and copy it to toPath name

            int error = 0, level = 0;
            var procs = ProcPath(fromPath, ref error, ref level, mustExist: true);
            if (error != 0)
                return -LibC.ENOENT;

            var fromProc = procs.Pop();
            var fromRec = fromProc.Item2;

            if (fromRec == null) return -LibC.ENOENT;  // From must exist
            if (fromRec.IsDirectory) return -LibC.EISDIR;  // Can't be a dir
            if (!fromRec.IsFile) return -LibC.EPERM;   // Must be a file
            if (fromRec.MaintLevel) return -LibC.EPERM;  // Not in a maint level

            int derror = 0, dlevel = 0;
            var dprocs = ProcPath(toPath, ref derror, ref dlevel, mustExist: false);
            if (derror != 0)
                return -LibC.ENOENT;

            var toProc = dprocs.Pop();
            var toRec = toProc.Item2;

            Byte[] newFile = null;

            NeoAssets.Mongo.NeoVirtFS par = null;

            if (toRec != null)
            {
                if (toRec.IsFile) return -LibC.EEXIST;      // If a file it must not exist
                if (!toRec.IsDirectory) return -LibC.EPERM; // Must be a dir otherwise
                if (toRec.MaintLevel) return -LibC.EPERM;  // Not in a maint level

                par = toRec;
                newFile = fromRec.Name;
            }
            else
            { // Doesn't exist, so this is our new file (par is it's parent)
                var parProc = dprocs.Pop();
                if (parProc.Item2 == null) return -LibC.ENOENT;

                par = parProc.Item2;
                newFile = toProc.Item1;

                if (par.MaintLevel) return -LibC.EPERM;  // Not in a maint level               
            }

            var newLink = fromRec.MakeLink(par, newFile);
            NeoVirtFSCol.InsertOne(newLink);

            Console.WriteLine($"Create File Link: {newLink.Name.GetString()}");

            return 0;
        }
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            if (verbosity > 5)
                Console.WriteLine($"Unlink {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist: true);
            if (error != 0)
                return -LibC.ENOENT;

            int version = 0;
            var newFile = procs.Pop();

            var parentRec = procs.Pop();
            if (parentRec.Item2 == null) return -LibC.ENOENT;
            var par = parentRec.Item2;

            if (par.MaintLevel) return -LibC.EPERM;

            if (newFile.Item2 != null)
            {
                // Must delete the file
                version = newFile.Item2.Version;
                Console.WriteLine($"File Deleted: {newFile.Item2.Name.GetString()} id={newFile.Item2._id} Version={version}");

                fileDelete(newFile.Item2, DeleteTypes.UNLINK);
            }

            return 0;
        }

        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target)
        {
            if (verbosity > 0)
                Console.WriteLine($"SymLink {path.GetString()} {target.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.SymLink(path, target);
        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer)
        {
            if (verbosity > 0)
                Console.WriteLine($"ReadLink {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.ReadLink(path, buffer);
        }


        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            if (verbosity > 5)
                Console.WriteLine($"Create {path.GetString()} Flags={flagString(fi.flags)}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist: false);
            if (error != 0)
            {
                Console.WriteLine($"Path gets error {error}");
                return -LibC.ENOENT;
            }

            // New files are always cache files, but might have existing file that needs to be deleted

            int version = 0;
            var newFile = procs.Pop();
            if (newFile.Item2 != null)
            {
                // Must delete the file
                version = newFile.Item2.Version;
                Console.WriteLine($"File Deleted: {newFile.Item2.Name.GetString()} id={newFile.Item2._id} Version={version}");             

                fileDelete(newFile.Item2, DeleteTypes.CREATE);
            }

            var parentRec = procs.Pop();
            if (parentRec.Item2 == null) return -LibC.ENOENT;
            var par = parentRec.Item2;

            if (par.MaintLevel) return -LibC.EPERM;
            
            // Create new record (new id) and insert
            var newRec = NeoAssets.Mongo.NeoVirtFS.CreateNewFile(par, newFile.Item1, path, mode);
            newRec.Version = version + 1;

            NeoVirtFSCol.InsertOne(newRec);

            //var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, newRec._id);
            //var insert = NeoVirtFSDeletedCol.ReplaceOneAsync(filter, newRec, options: new ReplaceOptions { IsUpsert = true });

            Console.WriteLine($"File Created: {newRec.Name.GetString()} id={newRec._id}");

            fi.fh = storeHandler(FileDescriptor.FileHandlerFactory(newRec, db, bac, bvol));

            var fds = DescriptorStore[fi.fh];
            return fds.Handler.Create(fds, mode, fi.flags);
        }
        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 5)
                Console.WriteLine($"Open {path.GetString()} Flags={flagString(fi.flags)}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist : true, isADir: false);
            if (error != 0)
            {
                Console.WriteLine($"Path gets error {error}");
                return -LibC.ENOENT;
            }

            var newFile = procs.Pop();
            if (newFile.Item2 == null) return -LibC.ENOENT;
            var fileRec = newFile.Item2;
            var oldFile = fileRec;

            var parentRec = procs.Pop();
            if (parentRec.Item2 == null) return -LibC.ENOENT;
            var par = parentRec.Item2;

            if (par.MaintLevel) return -LibC.EPERM;


            // O_TRUNC deletes the file, so we should delete it.
            if ((fi.flags & LibC.O_TRUNC)!=0)
            {
                // Must delete the file
                Console.WriteLine($"File Deleted: {newFile.Item2.Name.GetString()} id={newFile.Item2._id}  Version={newFile.Item2.Version}");
                fileDelete(newFile.Item2, DeleteTypes.TRUNC);

                fileRec = NeoAssets.Mongo.NeoVirtFS.CreateNewFile(par, newFile.Item1, path, (mode_t) (uint) oldFile.Stat.st_mode);
                fileRec.Version = oldFile.Version + 1;
                NeoVirtFSCol.InsertOne(fileRec);

                fi.flags |= LibC.O_CREAT;  // Otherwise it won't create the file again
            }
        
            fi.fh = storeHandler(FileDescriptor.FileHandlerFactory(fileRec, db, bac, bvol));

            var fds = DescriptorStore[fi.fh];
            return fds.Handler.Open(fds, fi.flags);
        }

        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            if (verbosity > 15)
                Console.WriteLine($"Read {path.GetString()}");

            var fds = DescriptorStore[fi.fh];
            return fds.Handler.Read(fds, offset, buffer);
        }
        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi)
        {
            if (verbosity > 15)
                Console.WriteLine($"Write {path.GetString()}");

            var fds = DescriptorStore[fi.fh];
            return fds.Handler.Write(fds, off, span);
        }
        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (verbosity > 10)
                Console.WriteLine($"Release {path.GetString()}");

            var fds = DescriptorStore[fi.fh];
            fds.Handler.Release(fds);

            releaseHandler(fds);
        }

        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            if (verbosity > 5)
                Console.WriteLine($"Truncate {path.GetString()} to {length}");


            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist: true);
            if (error != 0)
                return -LibC.ENOENT;

            var fromProc = procs.Pop();
            var fromRec = fromProc.Item2;

            return fromRec.Truncate(length); // Ironically the object doesn't know it's path
        }



        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            if (verbosity > 5)
                Console.WriteLine($"Rename {path.GetString()} to {newPath.GetString()}");

            // Rename's a bit link link, except it's literally changing the 'Name' on the
            // fromPath.   And the Name can also be a directory (you can rename a directory)

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level, mustExist: true);
            if (error != 0)
                return -LibC.ENOENT;

            var fromProc = procs.Pop();
            var fromRec = fromProc.Item2;

            //Console.WriteLine($"From: {fromProc.Item1.GetString()}");

            if (fromRec == null) return -LibC.ENOENT;  // From must exist
            //if (fromRec.IsDirectory) return -LibC.EISDIR;  // Can't be a dir
            //if (!fromRec.IsFile) return -LibC.EPERM;   // Must be a file
            if (fromRec.MaintLevel) return -LibC.EPERM;  // Not in a maint level

            int derror = 0, dlevel = 0;
            var dprocs = ProcPath(newPath, ref derror, ref dlevel, mustExist:false);
            if (derror != 0)
            {
                Console.WriteLine($" derror={derror}");
                return -LibC.ENOENT;
            }

            var toProc = dprocs.Pop();
            var toRec = toProc.Item2;

            //Console.WriteLine($"To: {toProc.Item1.GetString()}");

            Byte[] newFile = null;

            NeoAssets.Mongo.NeoVirtFS par = null;

            if (toRec != null && toRec.IsFile)  // This is permitted but we obliterate the Output File
            {
                //Console.WriteLine($" -- to file - remove output {toRec.Name.GetString()}");
                if (toRec.MaintLevel) return -LibC.EPERM;

                fileDelete(toRec, DeleteTypes.RENAMEOVER);

                var parProc = dprocs.Pop();
                var parrec = parProc.Item2;

                par = parrec;
                newFile = toRec.Name;
            }
            else
                if (toRec != null && toRec.IsDirectory)
            {
                //Console.WriteLine($" -- to dir - parent to new dir");

                if (toRec.MaintLevel) return -LibC.EPERM;  // Not in a maint level

                par = toRec;
                newFile = fromRec.Name;
            }
            else
            { // Doesn't exist, so this is our new file (par is it's parent)
              

                var parProc = dprocs.Pop();
                if (parProc.Item2 == null) return -LibC.ENOENT;

                par = parProc.Item2;
                newFile = toProc.Item1;

                if (par.MaintLevel) return -LibC.EPERM;  // Not in a maint level
                                                         // 
                //Console.WriteLine($" -- nonexist - parent to new dir");
            }

            NodeCacheInvalidate(fromRec);  // Old version's going away

            fromRec.Rename(par, newFile);

            var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, fromRec._id);
            var insert = NeoVirtFSCol.ReplaceOne(filter, fromRec, options: new ReplaceOptions { IsUpsert = false });        

            //Console.WriteLine($"Rename File: {fromRec.Name.GetString()} Match={insert.MatchedCount} Mod={insert.ModifiedCount}");

            return 0;
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
            var procs = ProcPath(path, ref error, ref level, mustExist: true);
            if (error != 0)
                return -LibC.ENOENT;

            var lev = procs.Pop();
            if (lev.Item2 == null) return -LibC.ENOENT;

            var attribs = lev.Item2.GetAttributes();

            return 0;
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            if (verbosity > 0)
                Console.WriteLine($"RemoveXAttr {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.RemoveXAttr(path, name);
        }
        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            if (verbosity > 0)
                Console.WriteLine($"GetXAttr {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.GetXAttr(path, name, data);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            if (verbosity > 0)
                Console.WriteLine($"SetXAttr {path.GetString()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;


            return base.SetXAttr(path, name, data, flags);
        }


        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            if (verbosity > 0)
                Console.WriteLine($"StatFS {path.GetString()}");

            int error = 0,level=0;
            var procs = ProcPath(path, ref error, ref level, isADir: true);
            if (error != 0)
                return -LibC.ENOENT;


            return base.StatFS(path, ref statfs);
        }

        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            if (verbosity > 0)
                Console.WriteLine($"UpdateTimestamps {path.GetString()} atime={atime.ToDTO()} mtime={atime.ToDTO()}");

            int error = 0, level = 0;
            var procs = ProcPath(path, ref error, ref level);
            if (error != 0)
                return -LibC.ENOENT;

            var ent = procs.Pop();
            if (ent.Item2 == null) return -LibC.ENOENT;

            var rec = ent.Item2;

            var update = new UpdateDefinitionBuilder<NeoAssets.Mongo.NeoVirtFS>()
                  .Set(rec => rec.Stat.st_atim, atime.ToDTO())
                  .Set(rec => rec.Stat.st_mtim, mtime.ToDTO());

            var result = NeoVirtFSCol.UpdateOne(Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, rec._id), update);

            return 0;
        }

        #endregion

        #region Helper Methods

        private string NodeKey(NeoAssets.Mongo.NeoVirtFS node)
        {
            var sb = new StringBuilder();

            sb.Append(node._id.ToString());
            sb.Append('_');
            sb.Append(Convert.ToHexString(node.Name));
            //Console.WriteLine($"KeyN: {sb}");

            return sb.ToString();
        }

        private string NodeKey(ObjectId id, byte[] name)
        {
            var sb = new StringBuilder();

            sb.Append(id.ToString());
            sb.Append('_');
            sb.Append(Convert.ToHexString(name));

            //Console.WriteLine($"KeyA: {sb}");

            return sb.ToString();
        }

        private void NodeCacheInvalidate(NeoAssets.Mongo.NeoVirtFS node)
        {
            var key = NodeKey(node);

            lock (nodeCache)
            {
                nodeCache.Remove(key);
            }
        }

        private NeoAssets.Mongo.NeoVirtFS? NodeCacheGet(ObjectId id, byte[] name)
        {
            var key = NodeKey(id, name);
            lock (nodeCache)
            {
                if (nodeCache.TryGetValue(key, out var resObj)){
                    var res = (NeoAssets.Mongo.NeoVirtFS) resObj;

                    return res;
                } 
            }

            return null;
        }

        private void NodeCacheSet(NeoAssets.Mongo.NeoVirtFS node, int expireMins)
        {
            var key = NodeKey(node);
            lock (nodeCache)
            {
                var exp = DateTimeOffset.Now.AddMinutes(expireMins);
                nodeCache.Set(key, node, exp);
            }
        }


        private void fileDelete(NeoAssets.Mongo.NeoVirtFS rec, DeleteTypes dType)
        {
            // Remove file node from main file, move to the deleted list

            // Don't forget to mark the dtim

            rec.Stat.st_dtim = DateTimeOffset.UtcNow;
            rec.DeleteType = dType;

            // Upsert to the deleted collection  - maybe this should be in a unit of work?

            var filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, rec._id);
            var insert = NeoVirtFSDeletedCol.ReplaceOneAsync(filter, rec, options: new ReplaceOptions { IsUpsert = true });

            // Remove from the original collection

            NeoVirtFSCol.DeleteOne(filter);
        }

        private void releaseHandler(FileDescriptor fds)
        {
            var fd = fds.fd;
            lock (DescriptorStore)
            {
                DescriptorFree[fd] = true;
                DescriptorStore.Remove(fd);
                Console.WriteLine($"[Destroy Handler {fd} Free={DescriptorFree.Count} Existing={DescriptorStore.Count}]");
            }
        }

        private ulong storeHandler(FileDescriptor fds)
        {
            lock (DescriptorStore)
            {
                ulong fd = 0;
                if (DescriptorFree.Count > 0)
                {
                    fd = DescriptorFree.Keys.First();
                    DescriptorFree.Remove(fd);

                    DescriptorStore[fd] = fds;
                    fds.fd = fd;

                    Console.WriteLine($"[Recycle Handler {fd} max={FileDescriptorMax} Free={DescriptorFree.Count} Existing={DescriptorStore.Count}]");

                    return fd;
                }

                fd = FileDescriptorMax++;
                DescriptorStore[fd] = fds;
                fds.fd = fd;

                Console.WriteLine($"[New Handler {fd} max={FileDescriptorMax} Free={DescriptorFree.Count} Existing={DescriptorStore.Count}]");
                return fd;
            }
        }

        private static NeoAssets.Mongo.NeoVirtFS? rootCache = null;  // Need to make true cache for this.  For not it's not likely to ever change

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
            //Console.WriteLine($"Path: {path.GetString()} {retA.Length}");
            //for (var i = 0; i < retA.Length; i++)
            //    Console.WriteLine($" {i}: {retA[i].GetString()}");

            // Get the Root level
            retId = RootNameSpace._id;

            FilterDefinition<NeoAssets.Mongo.NeoVirtFS>? filter = null;
            NeoAssets.Mongo.NeoVirtFS? node = null;

            if (rootCache == null)
            {

                //Console.WriteLine($"  - Get Root {retId}");
                filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x._id, retId);
                node = NeoVirtFSCol.FindSync(filter).FirstOrDefault();

                rootCache = node;

            }
            else node = rootCache;

            //Console.WriteLine($"Level: {retA.Length}");

            error = node == null ? LibC.ENOENT : 0;

            stack.Push(new Tuple<byte[], NeoAssets.Mongo.NeoVirtFS?, int>(Array.Empty<byte>(), node, error));

            outLevel = retA.Length;

            if (retA.Length < 1 || node == null)
                return stack;

            // Now we walk down

            var notFound = 0;
            int depth = 0;

            foreach (var level in retA)
            {
                depth++;

                var prevNode = node;
                node = NodeCacheGet(node._id, level);
                if (node == null)
                {

                    //Console.WriteLine($"  - Get {level.GetString()} Parent={node._id}");
                    filter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, prevNode._id) &
                         Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.Name, level);

                    node = NeoVirtFSCol.FindSync(filter).FirstOrDefault();
                    error = 0;

                    if (node != null) {
                        int minutes = 5;
                        switch (depth)
                        {
                            case 1:
                                minutes = 120;      // NameSpace level
                                break;
                            case 2:
                                minutes = 30;       // Volume Level
                                break;

                        }
                        NodeCacheSet(node, minutes);
                    }
                }

                if (node == null)
                {
                    error = LibC.ENOENT;
                    notFound++;
                }

                stack.Push(new Tuple<byte[], NeoAssets.Mongo.NeoVirtFS?, int>(level, node, error));
            }

           

            switch (notFound)
            {
                case 0:     // Everything found
                    error = 0;
                    break;
                case 1:     // Last thing found (which is typical)
                    if (mustExist)
                        error = LibC.ENOENT;
                    else
                        error = 0;
                    break;
                default:    // Multiple things not found - which is almost always bad
                    error = LibC.ENOENT;
                    break;
            }

            //Console.WriteLine($"Found: NotFound={notFound} Error={error}");

            return stack;
        }

        private string flagString(int flags)
        {
            var accmode = flags & LibC.O_ACCMODE;

            var mode = "O_RDONLY";
            if (accmode == LibC.O_WRONLY)
                mode = "O_WRONLY";
            else if (accmode == LibC.O_RDWR)
                mode = "O_RDWR";

            if ((flags & LibC.O_CREAT)!=0)
                mode += ", O_CREAT";
            if ((flags & LibC.O_EXCL) != 0)
                mode += ", O_EXCL";
            if ((flags & LibC.O_NOCTTY) != 0)
                mode += ", O_NOCTTY";
            if ((flags & LibC.O_TRUNC) != 0)
                mode += ", O_TRUNC";
            if ((flags & LibC.O_APPEND) != 0)
                mode += ", O_APPEND";
            if ((flags & LibC.O_NONBLOCK) != 0)
                mode += ", O_NONBLOCK";
            if ((flags & LibC.O_DSYNC) != 0)
                mode += ", O_DSYNC";
            if ((flags & LibC.FASYNC) != 0)
                mode += ", FASYNC";
            if ((flags & LibC.O_DIRECT) != 0)
                mode += ", O_DIRECT";
            if ((flags & LibC.O_LARGEFILE) != 0)
                mode += ", O_LARGEFILE";
            if ((flags & LibC.O_DIRECTORY) != 0)
                mode += ", O_DIRECTORY";
            if ((flags & LibC.O_NOFOLLOW) != 0)
                mode += ", O_NOFOLLOW";
            if ((flags & LibC.O_NOATIME) != 0)
                mode += ", O_NOATIME";
            if ((flags & LibC.O_CLOEXEC) != 0)
                mode += ", O_CLOEXEC";

            return mode;
        }


        #endregion
    }
}
