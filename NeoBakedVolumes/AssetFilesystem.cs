using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.Collections;
using System.Collections.Generic;
using NeoCommon;

namespace NeoBakedVolume
{

    // "0f49f6c69d7cecf96ec362dac94745c6425c98ee"; // Hello world achieved 2021/02/09

    public class AssetFilesystem : IFileProvider
    {
        private IMongoDatabase db;
        private IMongoCollection<BakedAssets> bac;

        public AssetFilesystem(IMongoDatabase db)
        {
            this.db = db;
            bac = db.BakedAssets();
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new BakedVolumeDirectories();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
           
            return new AssetFileSystem.AssetFile(subpath, db, bac);


            throw new NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }

        private class BakedVolumeDirectories : IDirectoryContents
        {
            // We've got to lie - 200million + "files" exist at this level - we'd never
            // be able to or even want to enumerate them
            bool IDirectoryContents.Exists => true;

            IEnumerator<IFileInfo> IEnumerable<IFileInfo>.GetEnumerator()
            {
                return (IEnumerator<IFileInfo>) new List<IFileInfo>();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator<IFileInfo>) new List<IFileInfo>();
            }
        }
    }
}

