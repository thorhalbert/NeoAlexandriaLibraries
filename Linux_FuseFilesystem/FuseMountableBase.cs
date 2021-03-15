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
    public class FuseMountableBase : FuseFileSystemBase
    {
        private IMongoCollection<AssetFiles> assetFiles = null;
        private IMongoCollection<BakedAssets> bakedAssets;
        private IMongoCollection<BakedVolumes> bakedVolumes;

        protected bool debug = true;


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


        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::GetXAttr()");
            return base.GetXAttr(path, name, data);
        }
        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::ListXAttr()");
            return base.ListXAttr(path, list);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::SetXAttr()");
            return base.SetXAttr(path, name, data, flags);
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            path = TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::RemoveXAttr()");
            return base.RemoveXAttr(path, name);
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

                if (debug) Console.WriteLine($"Return stat for: {fileuuid.ToString().ToLower()}");

                return;
            }

            Console.WriteLine($"Can't find AssetFile: {fileuuid.ToString().ToLower()}");
        }

        internal int AssetOpen(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            // Lazy load these, but just once
            if (bakedAssets == null)
                bakedAssets = NeoMongo.NeoDb.BakedAssets();
            if (bakedVolumes == null)
                bakedVolumes = NeoMongo.NeoDb.BakedVolumes();

            // "Open" the file -- mostly just setup
            var assetLink = new AssetFileSystem.File.UnbakeForFuse(NeoMongo.NeoDb, bakedAssets, bakedVolumes, fi.ExtAssetSha1);          
            fi.AssetLink = assetLink;  // Save in file context -- mostly needed by read

            return 1;   // Fake file handle - nothing should refer to it or use it
        }
        internal int AssetRead(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            var assetLink = (AssetFileSystem.File.UnbakeForFuse) fi.AssetLink;
            return assetLink.Read(offset, buffer);
        }
        internal void AssetRelease(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var assetLink = (AssetFileSystem.File.UnbakeForFuse) fi.AssetLink;
            assetLink.Release();   // Internally clear buffers and such

            fi.AssetLink = null;  // Let this go out of scope
            fi.ExtAssetSha1 = null;
        }

    }
}
