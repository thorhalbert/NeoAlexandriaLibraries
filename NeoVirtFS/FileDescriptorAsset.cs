using MongoDB.Driver;
using NeoAssets.Mongo;
using NeoBakedVolumes.Mongo;
using Tmds.Linux;
using static AssetFileSystem.File;

namespace NeoVirtFS
{
    internal class FileDescriptorAsset : FileDescriptor, INeoVirtFile
    {
        private UnbakeForFuse? asset;

        private readonly IMongoDatabase db;
        private readonly IMongoCollection<BakedAssets> bac;
        private readonly IMongoCollection<BakedVolumes> bVol;
        private readonly NeoAssets.Mongo.NeoVirtFS file;

        public FileDescriptorAsset(NeoAssets.Mongo.NeoVirtFS myFile,
                    MongoDB.Driver.IMongoDatabase db,
                    MongoDB.Driver.IMongoCollection<NeoBakedVolumes.Mongo.BakedAssets> bac,
                    MongoDB.Driver.IMongoCollection<NeoBakedVolumes.Mongo.BakedVolumes> bVol) : base(myFile)
        {
            // Just load this stuff - we become active if we get an open call

            asset = null;

            this.db = db;
            this.bac = bac;
            this.bVol = bVol;
            this.file = myFile;
        }

        public int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            // Virtual file system has to be able to overwrite baked files, I think that happens at a higher 
            // level than this though - we will reject writes

            return -LibC.EROFS;
        }

        public int Open(FileDescriptor fds, int flags)
        {
            try
            {
                asset = new UnbakeForFuse(db, bac, bVol, Convert.ToHexString(file.Content.AssetSHA1));
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Asset Open Error: {Convert.ToHexString(file.Content.AssetSHA1)} {ex.Message}");

                return -LibC.EIO;
            }
        }

        public int Read(FileDescriptor fds, ulong offset, Span<byte> buffer)
        {
            if (asset == null) return -LibC.EBADF;
            return asset.Read(offset, buffer);
        }

        public int Release(FileDescriptor fds)
        {
            if (asset == null) return -LibC.EBADF;

            asset.Release();
            return 0;
        }

        public int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span)
        {
            return -LibC.EROFS;
        }
    }

}
