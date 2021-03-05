using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using NeoCommon.VfsInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Linux;

namespace NeoBakedVolumes
{
    public class VfsBakedAssets : IVfsFilesystem
    {

        private IMongoDatabase db;
        private IMongoCollection<BakedAssets> bac;

        public VfsBakedAssets(IMongoDatabase db)
        {
            this.db = db;
            bac = db.BakedAssets();
        }

        IVfsDirectoryInfo IVfsFilesystem.DirectoryInfo(IVfsPath path)
        {
            var dir = new VfsBakedAsset_Directory(db, bac, this,path);
            var com = new VfsBakedAsset_Common(db, bac, this, dir, null, path);
            dir._attachCom(com);

            return dir;
        }

        IVfsFileInfo IVfsFilesystem.FileInfo(IVfsPath path)
        {
            var file = new VfsBakedAsset_FileInfo(db, bac, this, path);
            var com = new VfsBakedAsset_Common(db, bac, this, null, file, path);
            file._attachCom(com);

            return file;
        }

        int IVfsFilesystem.StatFS(IVfsPath path)
        {
            throw new NotImplementedException();
        }
    }
}
