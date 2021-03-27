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
using NeoCommon;
using System.Security.Cryptography;

namespace NeoBakedVolumes
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
                return (IEnumerator<IFileInfo>) new List<IFileInfo>();
            }
        }
    }
}

namespace AssetFileSystem
{
    public class File : IFileInfo
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

        public File(string subpath, IMongoDatabase db, IMongoCollection<BakedAssets> bac, IMongoCollection<BakedVolumes> bVol = null)
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
            return new GZipStream(new MakeStream(baRec, volRec, this), CompressionMode.Decompress);  // This seems to work too
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

                if (val.Length <= count)
                {
                    Array.Copy(val.ToArray(), buffer, val.Length);
                    //buffer = val.ToArray();
                    return val.Length;
                }

                // split buffer and build a prevbuffer - for next time

                var retBuf = val.Slice(0, count);
                prevBuffer = val.Slice(count);

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
        }

        public class UnbakeContext
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
            public ulong fileRead { get; private set; }
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

                fileRead = 0;

                // Part Info (the piece of the archive we're reading)

                currentPartRemain = volumelength - offsetx;
                currentFileNum = -1;
                currentPart = null;

                //Console.WriteLine($"Compressed={realLength} Actual={baRec.FileLength}");

                //bytesRead = 0;
            }

            public ReadOnlyMemory<byte> ReadNextBlock(uint size = 0)
            {
                // Figure out what system our primary connector is on
                var channel = GrpcChannel.ForAddress("http://feanor:5000",
                                    new GrpcChannelOptions
                                    {
                                        MaxReceiveMessageSize = null, // 5 MB
                                        MaxSendMessageSize = null // 2 MB
                                    });
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

                if (size > fileRemain)
                    size = (uint) fileRemain;

                // These files are guaranteed to be under 32 bits in length
                var returnVal = assetClient.Fetch(new FetchRequest()
                {
                    BakedVolume = volRec._id,
                    Part = currentPart,
                    Offset = (int) offsetx,
                    RequestCount = (int) size,
                });

                var length = returnVal.Length;
                fileRead += (ulong) length;

                //var dump = Utils.HexDump(returnVal.Payload.ToByteArray());
                //Console.WriteLine(dump);

                //Console.WriteLine($"Read: Part {currentFileNum}, Length {length} Read {fileRead} Remain {currentPartRemain} ");

                //using(var f = new BinaryWriter(System.IO.File.OpenWrite("test.gz")))
                //{
                //    f.Write(returnVal.Payload.ToByteArray());
                //}

                // Set variables for next time


                fileRemain -= (ulong) length;
                offsetx += (ulong) length;

                currentPartRemain -= (ulong) length;

                // See if we're spanning parts
                if (currentPartRemain < 1)
                {
                    FileNum++;

                   // Console.WriteLine($"[Proceed to next file {FileNum}]");

                    offsetx = 0;   // Start at beginning of next file

                    currentPartRemain = Math.Min(volumelength, realLength - fileRead);
                }

                return returnVal.Payload.Memory.Slice(0, length);
            }

            internal void Release()
            {
                // Let our GC happen quickly
            }
        }
        /// <summary>
        /// Helper class to move the basic mechanics of unbaking into this class
        /// and provide appropriate interfaces (and keep light context for the file
        /// open as needed)
        /// </summary>
        public class UnbakeForFuse
        {

            public bool Debug { get; set; } = false;

            IMongoDatabase db;
            public IMongoCollection<BakedAssets> bakedAssets { get; set; }

            private string assetId;
            //private SHA1 sha1Computer;
            //private SHA1 sha1Computer2;
            private File file;
            private ulong fileLength;

            public IMongoCollection<BakedVolumes> bakedVolumes { get; set; }
            public Stream assetStream { get; private set; }
            public ulong CurrentPosition { get; private set; }

           

            BakedVolumes volRec;
            BakedAssets baRec;
            bool atEof = false;

            byte[] bigBuffer = null;
            Memory<byte> bigMemory = null;
            Memory<byte> bigRemaining = new Memory<byte>(new byte[0]);
            //ulong bufferOffset = 0;
            //ulong bufferBytes = 0;


            public UnbakeForFuse(IMongoDatabase db, IMongoCollection<BakedAssets> bac, IMongoCollection<BakedVolumes> bVol, string assetId)
            {
                try
                {
                    this.db = db;
                    this.bakedAssets = bac;
                    this.assetId = assetId;

                    //sha1Computer = System.Security.Cryptography.SHA1.Create();

                    //sha1Computer2 = System.Security.Cryptography.SHA1.Create();


                    if (Debug) Console.WriteLine($"UnbakeForFuse::Construct Asset={assetId}");

                    // We must wrap the file stream since we need the un-gzip to happen

                    file = new File(assetId, db, bac);

                    fileLength = file.baRec.FileLength;

                    assetStream = file.CreateReadStream();

                    CurrentPosition = 0;
                    bigBuffer = null;  // Discard anything we have here
                    //bufferBytes = 0;
                    //bufferOffset = 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UnbakeForFuse::Construct {ex.Message} {ex.StackTrace}");
                }
            }


            public int Read(ulong offset, Span<byte> buffer)
            {
                try
                {
                    if (atEof) return 0;

                    if (Debug) Console.WriteLine($"UnbakeForFuse::Read(Offset={offset}, Length={buffer.Length})");

                    // Implement offset as if we are seekable
                    // Compute a forward gap and eat those bytes as we read forward
                    // If we get a negative gap, we rewind back to beginning and forward again

                    // We're going to read big chunks

                    if (bigBuffer == null)
                    {
                        bigBuffer = new byte[1 * 1024 * 1024];
                        bigMemory = bigBuffer.AsMemory();   // Make Memory block so we can do slices
                    }

                    // See if we need to rewind - we need to see how often this
                    // happens 
                    if (offset < CurrentPosition)
                    {
                        if (Debug) Console.WriteLine($"Stream Rewind Offset={offset} Current={CurrentPosition}");
                        RewindReopen();
                    }

                    // Bytes to skip over to accomplish offset
                    var SkipDelta = offset - CurrentPosition;
                    if (SkipDelta>0)
                        if (Debug) Console.WriteLine($"Need to Skip={SkipDelta} bytes");

                    // We may have to loop indefinately to fulfill SkipDelta
                    while (true)
                    {
                        // Either we underflow or we need to read more
                        if (bigRemaining.Length < buffer.Length)
                        {
                            // Console.WriteLine("Load a buffer");
                            // Load in some buffer if we don't already have it

                            var readBuffer = bigMemory.Slice(bigRemaining.Length);
                            bigRemaining.CopyTo(bigMemory);
                            var bufferBytes = assetStream.Read(readBuffer.Span);

                            //sha1Computer2.TransformBlock(readBuffer.ToArray(), 0, bufferBytes, readBuffer.ToArray(), 0);

                            var newTotal = bigRemaining.Length + bufferBytes;
                            if (Debug)
                                Console.WriteLine($"[Stream read - add = {bufferBytes}, new = {newTotal}]");

                            if (bufferBytes < 1)  // We hit EOF
                            {
                                CurrentPosition += (ulong) bigRemaining.Length;

                                if (Debug) Console.WriteLine($"[EOF - Length={bigRemaining.Length} - File Total={CurrentPosition}]");
                                if (CurrentPosition != fileLength)
                                    if (Debug) Console.WriteLine($"Length Mismatch - read={CurrentPosition} actual={fileLength}");
                                atEof = true;
                                bigRemaining.Span.CopyTo(buffer);

                                //sha1Computer.TransformBlock(bigRemaining.ToArray(), 0, bigRemaining.Length, bigRemaining.ToArray(), 0);

                                //sha1Computer.TransformFinalBlock(bigRemaining.ToArray(), 0, 0);

                                //sha1Computer2.TransformFinalBlock(bigRemaining.ToArray(), 0, 0);

                                //var hashin = sha1Computer2.Hash;

                                // The hashes aren't that interesting if system is skipping about

                                //var sb = new StringBuilder();
                                //for (var i = 0; i < hashin.Length; i++)
                                //    sb.Append(hashin[i].ToString("x2"));

                                //Console.WriteLine($"Input Hash = {sb}");

                                //var hashout = sha1Computer.Hash;

                                //sb = new StringBuilder();
                                //for (var i = 0; i < hashout.Length; i++)
                                //    sb.Append(hashout[i].ToString("x2"));

                                //Console.WriteLine($"Output Hash = {sb}");

                                //if (hashin != hashout)
                                //    Console.WriteLine("[Hash Mismatch]");

                                return bigRemaining.Length;
                            }

                            bigRemaining = new Memory<byte>(bigBuffer, 0, newTotal);
                        }

                        // SkipDelta bigger than what's left -- eat it and get more
                        if (bigRemaining.Length < (int) SkipDelta)
                        {
                            if (Debug) Console.WriteLine($"Skip All Bytes in buffer remain={bigRemaining.Length} < skip={SkipDelta}");
                            CurrentPosition += (ulong) bigRemaining.Length;  // Move high water level
                            SkipDelta -= (ulong) bigRemaining.Length;        // We've accomplished this much of offset

                            bigRemaining = new Memory<byte>(new byte[0]);  // Flush buffer

                            continue;   // Back for more 
                        }

                        // Cut off SkipDelta
                        if (SkipDelta > 0)
                        {
                            if (Debug) Console.WriteLine($"Slice off {SkipDelta} at beginning");
                            bigRemaining = bigRemaining.Slice((int) SkipDelta);
                            CurrentPosition += (ulong) SkipDelta;
                        }

                        // Are we equal to or bigger than the buffer
                        var retBytes = Math.Min(bigRemaining.Length, buffer.Length);

                        var copyBuf = bigRemaining.Slice(0, retBytes);
                        bigRemaining = bigRemaining.Slice(retBytes);

                        copyBuf.Span.CopyTo(buffer);

                        CurrentPosition += (ulong) retBytes;

                        if (Debug) Console.WriteLine($"[Return {retBytes} bytes - carry forward {bigRemaining.Length} - Current {CurrentPosition}");

                        //sha1Computer.TransformBlock(copyBuf.ToArray(), 0, copyBuf.Length, copyBuf.ToArray(), 0);

                        return (int) retBytes;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UnbakeForFuse::Read {ex.Message} {ex.StackTrace}");
                    return 0;
                }
            }

            /// <summary>
            /// We've seen an offset that needs us to rewind.
            /// Rewind and go back to the beginning.
            /// </summary>
            private void RewindReopen()
            {
                try
                {
                    if (Debug) Console.WriteLine("[Reopen stream]");
                    assetStream.Close();
         

                    file = new File(assetId, db, bakedAssets);

                    fileLength = file.baRec.FileLength;

                    assetStream = file.CreateReadStream();

                    CurrentPosition = 0;
                   
                    bigRemaining = new Memory<byte>(new byte[0]);
                  
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UnbakeForFuse::RewindReopen {ex.Message} {ex.StackTrace}");
                }
            }
            public void Release()
            {
                // Clear/Destroy any buffers we have outstanding

                bigBuffer = null;
                assetStream.Close();
                file = null;
            }
        }
    }
}