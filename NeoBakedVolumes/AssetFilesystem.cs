using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoBakedVolumes
{
    class AssetFilesystem : IFileProvider
    {
        private MongoDatabaseBase db;
        private IMongoCollection<BakedAssets> bac;

        public AssetFilesystem(MongoDatabaseBase db)
        {
            this.db = db;
            bac = db.BakedAssets();
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {

            return new AssetFileSystem.File(subpath, db, bac);


            throw new NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }
    }
}

namespace AssetFileSystem
{
    class File : IFileInfo
    {
        private string subpath;
        private MongoDatabaseBase db;
        private IMongoCollection<BakedAssets> bac;

        private BakedAssets baRec = null;
        private BakedVolumes volRec;

        public File(string subpath, MongoDatabaseBase db, IMongoCollection<BakedAssets> bac)
        {
            this.db = db;
            this.bac = bac;
            this.subpath = subpath;

            var theFilter = Builders<BakedAssets>.Filter.Eq("_id", subpath);
            baRec = bac.FindSync(theFilter).FirstOrDefault();

            // This file could conceivably exist as an unanneled file but we would
            // need a different abstraction here to read it -- we can generate a different
            // Stream, but we will need a different file service
            // Also this scheme needs access controls
            if (baRec == null) return;

            var vCol = db.BakedVolumes();

            var volFilter = Builders<BakedVolumes>.Filter.Eq("_id", baRec.Volume);
            volRec = vCol.FindSync(volFilter).FirstOrDefault();
        }

        public bool Exists => baRec != null;

        public long Length => (long)baRec.RealLength;

        public string PhysicalPath => subpath;

        public string Name => subpath;

        public DateTimeOffset LastModified => DateTimeOffset.FromUnixTimeSeconds((long)baRec.CTime);

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            if (!baRec.Annealed.Value)
                throw new NotImplementedException();

            var compression = baRec.Comp;
            var volume = baRec.Volume;

            var block = baRec.Block;
            var startOffset = baRec.Offset;

            return new MakeStream(baRec, volRec);
        }

        private class MakeStream : Stream
        {
            private BakedAssets baRec;
            private BakedVolumes volRec;

            public MakeStream(BakedAssets baRec, BakedVolumes volRec)
            {
                this.baRec = baRec;
                this.volRec = volRec;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;   // At least for now - if we can cache we can make this work

            public override bool CanWrite => false;   // Can never really do this - at least for an asset - they're baked

            public override long Length => (long) baRec.RealLength;  // Why would length ever be negative (not ulong?)

            // Hopefully covered with CanSeek=false
            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void Flush()
            {
                // Flush would be for write so no problem
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }
}