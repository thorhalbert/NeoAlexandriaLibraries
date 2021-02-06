using BakedFileService.Protos;
using Grpc.Net.Client;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace NeoBakedVolumes
{
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

            return new AssetFileSystem.File(subpath, db, bac);


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
                return (IEnumerator<IFileInfo>)new List<IFileInfo>();
}
        }
    }
}

namespace AssetFileSystem
{
    class File : IFileInfo
    {
        private string subpath;
        private IMongoDatabase db;
        private IMongoCollection<BakedAssets> bac;

        private BakedAssets baRec = null;
        private BakedVolumes volRec;

        private UnbakeContext unBaker { get; set; }

        internal static BakedVolumeData.BakedVolumeDataClient assetClient;

        ulong real_position = 0;
        ulong decomp_position = 0;

        public File(string subpath, IMongoDatabase db, IMongoCollection<BakedAssets> bac)
        {
            this.db = db;
            this.bac = bac;
            this.subpath = subpath;

            // We may want to enforce a file heirarchy here, but for not just need an
            // asset record to match (key is simply the sha1 in lower case and text)

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

            unBaker = new UnbakeContext(db, volRec, baRec);


            //var channel = GrpcChannel.ForAddress("http://feanor:5000");
            //assetClient = new BakedVolumeData.BakedVolumeDataClient(channel);
        }

        public bool Exists => baRec != null;

        public long Length => (long)baRec.RealLength;

        public string PhysicalPath => subpath;

        public string Name => subpath;

        public DateTimeOffset LastModified => DateTimeOffset.FromUnixTimeSeconds((long)baRec.CTime);

        public bool IsDirectory => false;

        private string call_Volume;
        private string call_Part;
        private uint call_Block;
        private uint call_Offset;
        private DeflateStream call_Deflate;




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

            return new GZipStream(new MakeStream(baRec, volRec, this), CompressionMode.Decompress);
        }

        private class MakeStream : Stream
        {
            private BakedAssets baRec;
            private BakedVolumes volRec;
            private File fileCreator;

            public MakeStream(BakedAssets baRec, BakedVolumes volRec, File file)
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
                    val = fileCreator.unBaker.ReadNextBlock();                
                }
                else  // Else get rest of prevbuffer
                {
                    val = prevBuffer;
                    prevBuffer = new ReadOnlyMemory<byte>();
                }

                if (val.Length<=count)
                {
                    buffer = val.ToArray();
                    return val.Length;
                }

                // split buffer and build a prevbuffer - for next time

                var retBuf = val.Slice(0, count);
                prevBuffer = val.Slice(count);

                buffer = retBuf.ToArray();
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
        }

        private class UnbakeContext
        {
            private IMongoDatabase db;
            private BakedVolumes volRec;
            private BakedAssets baRec;

            private const uint BUFFSIZE = 4 * 1024 * 1024;

            public ulong volumelength { get; private set; }
            public uint block { get; private set; }
            public uint address { get; private set; }
            public int FileNum { get; private set; }
            public ulong offsetx { get; private set; }
            public ulong remain { get; private set; }
            public ulong realLength { get; private set; }
            public ulong fileRemain { get; private set; }
            public ulong currentPartRemain { get; private set; }
            public int currentFileNum { get; private set; }
            public string currentPart { get; private set; }

            public UnbakeContext(IMongoDatabase db, BakedVolumes volRec, BakedAssets baRec)
            {
                this.db = db;
                this.volRec = volRec;
                this.baRec = baRec;
  
                volumelength = volRec.ArchSize / volRec.NumParts;

                // Read starting settings

                block = baRec.Block + 1;
                address = block << 9;

                FileNum = Convert.ToInt32(address / volumelength);

                offsetx = address % volumelength;
                remain = volumelength - offsetx;

                // File Info (meaning the file we're pullout out)

                realLength = baRec.RealLength;
                fileRemain = realLength;

                // Part Info (the piece of the archive we're reading)

                currentPartRemain = volumelength - offsetx;
                currentFileNum = -1;
                currentPart = null;

                //bytesRead = 0;
            }

            public ReadOnlyMemory<byte> ReadNextBlock(uint size=0)
            {
                // Figure out what system our primary connector is on
                var channel = GrpcChannel.ForAddress("http://feanor:5000");
                assetClient = new BakedVolumeData.BakedVolumeDataClient(channel);

                if (fileRemain < 1)
                    return new ReadOnlyMemory<byte>(); //EOF
                
                if (size < 1)
                    size = BUFFSIZE;

                if (FileNum != currentFileNum)
                {
                    // Close the old file, which we don't really have to do -- the service cache does that

                    // Open the new, which again the service does, we just need to say what
                    currentFileNum = FileNum;
                    currentPart = Convert.ToChar(currentFileNum + 65).ToString();  // File 0=A
                }

                // These files are guaranteed to be under 32 bits in length
                var returnVal = assetClient.Fetch(new FetchRequest()
                {
                    BakedVolume = volRec._id,
                    Part = currentPart,
                    Offset = (int) offsetx,
                    RequestCount = (int) size,
                });

                // Set variables for next time

                var  length = returnVal.Length;
                fileRemain -= (ulong) length;

                currentPartRemain -= (ulong) length;

                // See if we're spanning parts
                if (currentPartRemain < 1)
                {
                    currentFileNum++;
                    offsetx = 0;   // Start at beginning of next file

                    currentPartRemain = volumelength;
                }

                return returnVal.Payload.Memory;
            }


  
        }
    }
}