using BakedFileService.Protos;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.IO;
//using System.IO.Compression;

namespace AssetFileSystem
{
    public partial class AssetFile : IFileInfo
    {
        private string subpath;
        private IMongoDatabase db;
        private IMongoCollection<BakedAssets> bac;

        private BakedAssets baRec = null;
        private BakedVolumes volRec;

        private UnbakeContext unBaker { get; set; }

        internal static BakedVolumeData.BakedVolumeDataClient assetClient;

        //ulong real_position = 0;
        //ulong decomp_position = 0;

        public AssetFile(string subpath, IMongoDatabase db, IMongoCollection<BakedAssets> bac, IMongoCollection<BakedVolumes> bVol = null)
        {
            this.db = db;
            this.bac = bac;
            this.subpath = subpath.ToLowerInvariant();   // These come in upper case occasionally

            // We may want to enforce a file heirarchy here, but for not just need an
            // asset record to match (key is simply the sha1 in lower case and text)

            var theFilter = Builders<BakedAssets>.Filter.Eq("_id", this.subpath);
            baRec = bac.FindSync(theFilter).FirstOrDefault();

            // This file could conceivably exist as an unanneled file but we would
            // need a different abstraction here to read it -- we can generate a different
            // Stream, but we will need a different file service
            // Also this scheme needs access controls
            if (baRec == null)
            {
                Console.WriteLine($"Can't file BakedAsset: _id: {this.subpath}");
                return;
            }

            var vCol = bVol;
            if (vCol == null)
                vCol = db.BakedVolumes();

            try
            {
                var volFilter = Builders<BakedVolumes>.Filter.Eq("_id", baRec.Volume);
                volRec = vCol.FindSync(volFilter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: ${ex.Message}");
            }

            unBaker = new UnbakeContext(db, volRec, baRec);

            //var channel = GrpcChannel.ForAddress("http://feanor:5000");
            //assetClient = new BakedVolumeData.BakedVolumeDataClient(channel);
        }

        public bool Exists => baRec != null;

        public long Length => (long) baRec.RealLength;

        public string PhysicalPath => subpath;

        public string Name => subpath;

        public DateTimeOffset LastModified => DateTimeOffset.FromUnixTimeSeconds((long) baRec.CTime);

        public bool IsDirectory => false;

        private string call_Volume;
        //private string call_Part;
        private uint call_Block;
        private uint call_Offset;
        //private DeflateStream call_Deflate;


        private void Release()
        {
            if (unBaker!=null)
                            unBaker.Release();

            unBaker = null;
        }

        public Stream CreateReadStream()
        {
            if (!baRec.Annealed.Value)
                throw new NotImplementedException();

            var compression = baRec.Comp;
            if (compression != "GZIP")
                throw new ArgumentException("Unknown Compression: " + compression);

            call_Volume = baRec.Volume;
            call_Block = baRec.Block;
            call_Offset = baRec.Offset;

            //return new MakeStream(baRec, volRec, this);
            //return new ICSharpCode.SharpZipLib.GZip.GZipInputStream(new MakeStream(baRec, volRec, this)); - this actually doesn't work
            var innerStream = new MakeStream(baRec, volRec, this);

            return new System.IO.Compression.GZipStream(innerStream, System.IO.Compression.CompressionMode.Decompress);  // This seems to work too
            //return SharpCompress.Readers.GZip.GZipReader.Open(innerStream).OpenEntryStream();
            //return new SharpCompress.Compressors.Deflate.GZipStream(innerStream, SharpCompress.Compressors.CompressionMode.Decompress);

            //Console.WriteLine("Using SharpCompress.Readers.GZip.GZipReader.Open");
        }

        private class MakeStream : Stream
        {
            private BakedAssets baRec;
            private BakedVolumes volRec;
            private AssetFile fileCreator;

            public MakeStream(BakedAssets baRec, BakedVolumes volRec, AssetFile file)
            {
                this.baRec = baRec;
                this.volRec = volRec;
                fileCreator = file;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;   // At least for now - if we can cache we can make this work

            public override bool CanWrite => false;   // Can never really do this - at least for an asset - they're baked

            public override long Length => (long) baRec.FileLength;  // Why would length ever be negative (not ulong?)

            // Hopefully covered with CanSeek=false
            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void Flush()
            {
                // Flush would be for write so no problem
            }

            ReadOnlyMemory<byte> prevBuffer = new ReadOnlyMemory<byte>();
            public override int Read(byte[] buffer, int offset, int count)
            {
                ReadOnlyMemory<byte> val;

                if (prevBuffer.Length < 1) // If no prevbuffer then get next
                {
                    val = fileCreator.unBaker.ReadNextBlockDirect();
                }
                else  // Else get rest of prevbuffer
                {
                    val = prevBuffer;
                    prevBuffer = new ReadOnlyMemory<byte>();
                }

                if (val.Length <= count)
                {
                    Array.Copy(val.ToArray(), buffer, val.Length);
                    //buffer = val.ToArray();
                    return val.Length;
                }

                // split buffer and build a prevbuffer - for next time

                var retBuf = val[..count];
                prevBuffer = val[count..];

                Array.Copy(retBuf.ToArray(), buffer, count);

                //buffer.CopyTo(retBuf);
                return retBuf.Length;
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

            public override void Close()
            {
                fileCreator.unBaker.Release();
                base.Close();
            }
        }

     
    }
}