using Logos.Utility;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using NeoCommon;
using NeoRepositories.Mongo;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Fuse;
using Tmds.Linux;

namespace Linux_FuseFilesystem
{
    public class FuseMountableBase
    {
        private IMongoCollection<AssetFiles> assetFiles = null;
        private IMongoCollection<BakedAssets> bakedAssets;
        private IMongoCollection<BakedVolumes> bakedVolumes;

        protected bool debug = false;

        public Dictionary<ulong, FileContext> FileContexts { get; set; }


        public FuseMountableBase()
        {


            //Guid g;

            //g = GuidUtility.Create(GuidUtility.DnsNamespace, "python.org");
            //Console.WriteLine($"{g.ToString()} = '886313e1-3b8a-5372-9b90-0c9aee199e5d'");

            //g = GuidUtility.Create(GuidUtility.UrlNamespace, "http://python.org/");
            //Console.WriteLine($"{g.ToString()} = '4c565f0d-3f5a-5890-b41b-20cf47701c5e'");

            //g = GuidUtility.Create(GuidUtility.IsoOidNamespace, "1.3.6.1");
            //Console.WriteLine($"{g.ToString()} = '1447fa61-5277-5fef-a9b3-fbc6e44f4af3'");

            //g = GuidUtility.Create(new Guid("6ba7b814-9dad-11d1-80b4-00c04fd430c8"), "c=ca");
            //Console.WriteLine($"{g.ToString()} =  'cc957dd1-a972-5349-98cd-874190002798'");

        }
        internal ReadOnlySpan<byte> TransformPath(ReadOnlySpan<byte> path)
        {
            return path;
        }


        public virtual int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data, Guid fileGuid)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::GetXAttr()");

            return -LibC.ENOSYS;
        }
        public virtual int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list, Guid fileGuid)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::ListXAttr()");
            return -LibC.ENOSYS;
        }
        public virtual int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags, Guid fileGuid)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::SetXAttr()");
            return -LibC.ENOSYS;
        }
        public virtual int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Guid fileGuid)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::RemoveXAttr()");
            return -LibC.ENOSYS;
        }

        internal void GetAssetAttr(ReadOnlySpan<byte> path, Span<byte> link, ref stat stat, Guid fileuuid)
        {
            // Even though we have the id of the asset, we actually need the entry from BakedFiles,
            // which should have the last stat on it - or we use FileInfo in the sql database - maybe we'll do both
            // The asset might exist as lots of different files with lots of date (though only one size)

            if (debug) Console.WriteLine($"Found Tag: {RawDirs.HR(path)}={RawDirs.HR(link)}");

            // Had to find/build a python compatible uuid5 -- the guidex module does not make valid uuid5s
            //var fileuuid = Guid.Empty;
            //if (fiRef.ExtFileHandle.HasValue)
            //    fileuuid = fiRef.ExtFileHandle.Value;

            if (assetFiles == null)
                assetFiles = NeoMongo.NeoDb.AssetFiles();

            AssetFiles aRec = null;
            try
            {
                var volFilter = Builders<AssetFiles>.Filter.Eq("_id", fileuuid.ToString().ToLower());
                aRec = assetFiles.FindSync(volFilter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: ${ex.Message}");
            }

            if (aRec != null)
            {
                // These should be synthesized - eventually we'll have more security here - for now 
                //   all owned by neo:neo and world readable
                //stat.st_mode = (mode_t) aRec.Stat.mode;
                //stat.st_mode = S_IFREG | 0b100_100_100; // r--r--r--
                stat.st_nlink = 1;
                stat.st_mode = LibC.S_IFREG | (mode_t) 0b100_100_100;   // 444 protection for now
                stat.st_uid = 10010; // aRec.Stat.uid;
                stat.st_gid = 10010; // aRec.Stat.gid;

                if (aRec.Stat != null)
                {
                    stat.st_size = aRec.Stat.size;

                    stat.st_ctim = new timespec
                    {
                        tv_sec = aRec.Stat.ctime,
                        tv_nsec = 0
                    };
                    stat.st_mtim = new timespec
                    {
                        tv_sec = aRec.Stat.mtime,
                        tv_nsec = 0
                    };
                    stat.st_atim = new timespec
                    {
                        tv_sec = aRec.Stat.atime,
                        tv_nsec = 0
                    };
                }
                else
                {
                    // Just turn the link into our stat, we'll just keep the dates and make it into a file
                    stat.st_nlink = 1;
                    stat.st_mode = LibC.S_IFREG | (mode_t) 0b100_100_100;   // 444 protection for now
                    stat.st_uid = 10010; // aRec.Stat.uid;
                    stat.st_gid = 10010; // aRec.Stat.gid;

                    Console.WriteLine($"No internal Stat: {RawDirs.HR(path)}={RawDirs.HR(link)}");
                }

                if (debug) Console.WriteLine($"Stat Asset Dump: size={stat.st_size}, mode={stat.st_mode}, mtim={stat.st_mtim}");
                if (debug) Console.WriteLine($"Return stat for: {fileuuid.ToString().ToLower()}");

                return;
            }

            Console.WriteLine($"Can't find AssetFile: {fileuuid.ToString().ToLower()}");
        }

        internal int AssetOpen(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            if (FileContexts.TryGetValue(fi.fh, out var context))
            {

                // Lazy load these, but just once
                if (bakedAssets == null)
                    bakedAssets = NeoMongo.NeoDb.BakedAssets();
                if (bakedVolumes == null)
                    bakedVolumes = NeoMongo.NeoDb.BakedVolumes();

                //Console.WriteLine($"AssetOpen({RawDirs.HR(path)})");
                // "Open" the file -- mostly just setup
                var assetLink = new AssetFileSystem.AssetFile.UnbakeForFuse(NeoMongo.NeoDb, bakedAssets, bakedVolumes, context.ExtAssetSha1);
                context.AssetLink = assetLink;  // Save in file context -- mostly needed by read
                //Console.WriteLine($"Attach AssetLink to {fi.fh}");

                return 0;  // No error
            }
            else
                Console.WriteLine($"AssetOpen no context?  fh={fi.fh}");
            return 0;
        }
        internal int AssetRead(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi, Guid fileGuid)
        {
            if (FileContexts.TryGetValue(fi.fh, out var context))
            {
                var assetLink = context.AssetLink;
                return assetLink.Read(offset, buffer);
            }
            else
                Console.WriteLine($"AssetRead no context?  fh={fi.fh}");
            return 0;
        }
        internal void AssetRelease(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            if (FileContexts.TryGetValue(fi.fh, out var context))
            {
                if (context.AssetLink!=null)
                {
                    //Console.WriteLine($"AssetRelease({RawDirs.HR(path)})");
                    var assetLink = context.AssetLink;
                    assetLink.Release();   // Internally clear buffers and such
                }
                else
                {
                    Console.WriteLine($"AssetLink is null? {RawDirs.HR(path)}");
                }
                context.AssetLink = null;  // Let this go out of scope
                context.ExtAssetSha1 = null;
            }
            else
                Console.WriteLine($"AssetRelease no context?  fh={fi.fh}");
        }




        public virtual int Access(ReadOnlySpan<byte> path, mode_t mode, Guid fileGuid)
          => -LibC.ENOSYS;

        public virtual int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual void Dispose()
        { }

        public virtual int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int FSyncDir(ReadOnlySpan<byte> readOnlySpan, bool onlyData, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef, Guid fileGuid)
            => -LibC.ENOSYS;



        public virtual int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath, Guid fileGuid)
            => -LibC.ENOSYS;



        public virtual int MkDir(ReadOnlySpan<byte> path, mode_t mode, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
            => 0;

        public virtual int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        { }

        public virtual int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;


        public virtual int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int RmDir(ReadOnlySpan<byte> path, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int Unlink(ReadOnlySpan<byte> path, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef, Guid fileGuid)
            => -LibC.ENOSYS;

        public virtual int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi, Guid fileGuid)
            => -LibC.ENOSYS;
    }
}
