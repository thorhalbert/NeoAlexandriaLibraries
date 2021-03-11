using MongoDB.Driver;
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
        MongoDB.Driver.IMongoCollection<AssetFiles> assetFiles = null;
        internal ReadOnlySpan<byte> TransformPath(ReadOnlySpan<byte> path)
        {
            return path;
        }


        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::GetXAttr()");
            return base.GetXAttr(path, name, data);
        }
        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::ListXAttr()");
            return base.ListXAttr(path, list);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::SetXAttr()");
            return base.SetXAttr(path, name, data, flags);
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::RemoveXAttr()");
            return base.RemoveXAttr(path, name);
        }

        internal void GetAssetAttr(ReadOnlySpan<byte> path, Span<byte> link, stat stat, FuseFileInfoRef fiRef)
        {
            // Even though we have the id of the asset, we actually need the entry from BakedFiles,
            // which should have the last stat on it - or we use FileInfo in the sql database - maybe we'll do both
            // The asset might exist as lots of different files with lots of date (though only one size)

            Console.WriteLine($"Found Tag: {RawDirs.HR(path)}={RawDirs.HR(link)}");

            // Luckily I had sources here so I made it so I could path a byte path rather than a string
            var fileuuid = GuidEx.NewGuid(path.ToArray(), new Guid(GuidEx.NameSpaceUrl));

            if (assetFiles==null)
                assetFiles = NeoCommon.NeoMongo.NeoDb.AssetFiles();

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

                // These should be synthesized
                stat.st_mode = LibC.S_IFREG | (mode_t) 0b100_100_100;   // 444 protection for now
                stat.st_uid = aRec.Stat.uid;
                stat.st_gid = aRec.Stat.gid;

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

                return;
            }

            Console.WriteLine($"Can't find AssetFile: {fileuuid.ToString().ToLower()}");
        }
    }
}
