using MongoDB.Driver;
using NeoCommon.VfsInterfaces;
using System;

namespace NeoBakedVolumes
{
    internal class VfsBakedAsset_FS
    {
        private IMongoDatabase db;
        private VfsBakedAssets vfsBakedAssets;
        private IVfsPath path;

        public VfsBakedAsset_FS(IMongoDatabase db, VfsBakedAssets vfsBakedAssets, IVfsPath path)
        {
            this.db = db;
            this.vfsBakedAssets = vfsBakedAssets;
            this.path = path;
        }
    }
}