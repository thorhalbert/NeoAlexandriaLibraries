using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using SharpCompress.Compressors.Deflate;
using NeoBakedVolumes;

namespace AssetFileSystem
{
    public partial class AssetFile
    {
        /// <summary>
        /// Helper class to move the basic mechanics of unbaking into this class
        /// and provide appropriate interfaces (and keep light context for the file
        /// open as needed)
        /// </summary>
        public class UnbakeForFuse
        {

            public bool Debug { get; set; } = false;
            public bool DoSum { get; set; } = false;

            IMongoDatabase db;
            public IMongoCollection<BakedAssets> bakedAssets { get; set; }

            private string assetId;
            private SHA1 sha1Computer;
            private SHA1 sha1Computer2;
            private AssetFile file;
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


            public UnbakeForFuse(IMongoDatabase db, IMongoCollection<BakedAssets> bac, IMongoCollection<BakedVolumes> bVol, string assetId, bool assetDebug=false)
            {
                Debug = assetDebug;

                try
                {
                    this.db = db;
                    this.bakedAssets = bac;
                    this.assetId = assetId;

                    if (DoSum)
                    {
                        sha1Computer = System.Security.Cryptography.SHA1.Create();
                        sha1Computer2 = System.Security.Cryptography.SHA1.Create();
                    }

                    if (Debug) Console.WriteLine($"UnbakeForFuse::Construct Asset={assetId}");

                    // We must wrap the file stream since we need the un-gzip to happen

                    file = new AssetFile(assetId, db, bac);

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
                    if (Debug) Console.WriteLine($"UnbakeForFuse::Read(Offset={offset}, Length={buffer.Length} Current={CurrentPosition})");

                    //if (atEof) return 0;

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
                            // Load in some buffer if we don't already have it - copy what's left of bigremaining to bigMemory and add to end

                            var readBuffer = bigMemory[bigRemaining.Length..];
                            bigRemaining.CopyTo(bigMemory);
                            var bufferBytes = assetStream.Read(readBuffer.Span);

                            if (DoSum)
                            sha1Computer2.TransformBlock(readBuffer.ToArray(), 0, bufferBytes, readBuffer.ToArray(), 0);

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

                                if (DoSum)
                                {
                                    sha1Computer.TransformBlock(bigRemaining.ToArray(), 0, bigRemaining.Length, bigRemaining.ToArray(), 0);

                                    sha1Computer.TransformFinalBlock(bigRemaining.ToArray(), 0, 0);

                                    sha1Computer2.TransformFinalBlock(bigRemaining.ToArray(), 0, 0);

                                    var hashin = sha1Computer2.Hash;

                                    // The hashes aren't that interesting if system is skipping about

                                    var sb = new StringBuilder();
                                    for (var i = 0; i < hashin.Length; i++)
                                        sb.Append(hashin[i].ToString("x2"));

                                    Console.WriteLine($"Input Hash = {sb}");

                                    var hashout = sha1Computer.Hash;

                                    sb = new StringBuilder();
                                    for (var i = 0; i < hashout.Length; i++)
                                        sb.Append(hashout[i].ToString("x2"));

                                    Console.WriteLine($"Output Hash = {sb}");

                                    if (hashin != hashout)
                                        Console.WriteLine("[Hash Mismatch]");
                                }
                               
                                return bigRemaining.Length;
                            }

                            // Restate bigRemaining from the main buffer with new total
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

                        if (DoSum)
                            sha1Computer.TransformBlock(copyBuf.ToArray(), 0, copyBuf.Length, copyBuf.ToArray(), 0);

                        return (int) retBytes;
                    }
                }
                catch (InvalidDataException ide)
                {
                    Console.WriteLine($"UnbakeForFuse::Read Gzip decompression issue - {ide.Message} Id {assetId} - offset {offset}");
                    markError();
                    return -1;
                }
                catch (ZlibException zx)
                {
                    Console.WriteLine($"UnbakeForFuse::ZlibException - {zx.Message}");
                    markError();
                    return -1; // Needs to go up
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UnbakeForFuse::Read Error ({ex.GetType().Name}) - {ex.Message} {ex.StackTrace}");
                    return -1;
                }
            }

            private void markError()
            {
                bakedVolumes = db.BakedVolumes();
                bakedAssets = db.BakedAssets();

                var vMulti = Builders<BakedVolumes>.Update
                    .Set(u => u.ErrorDecompressing, true);

                var vFilter = Builders<BakedVolumes>.Filter.Eq(x => x._id, file.volRec._id);

                bakedVolumes.UpdateOne(vFilter, vMulti);

                var bMulti = Builders<BakedAssets>.Update
                  .Set(u => u.ErrorDecompressing, true);

                var bFilter = Builders<BakedAssets>.Filter.Eq(x => x._id, file.baRec._id);

                bakedAssets.UpdateOne(bFilter, bMulti);

                Console.WriteLine($"Mark Volume In Trouble: {file.volRec._id}, {file.baRec._id}");

                var err = new VolumeAssetDecodeError(file.volRec._id, file.baRec._id);
                err.SendMessage();
            }

            public class VolumeAssetDecodeError : NeoErrors
            {
                public string Volume { get; set; }
                public string AssetSha1 { get; set; }

                public VolumeAssetDecodeError(string volume, string assetsha1) : base()
                {                   
                    MessageType = "VolumeAssetDecodeError";

                    Volume = volume;
                    AssetSha1 = assetsha1;
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
         

                    file = new AssetFile(assetId, db, bakedAssets);

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
                if (assetStream!=null)   
                    assetStream.Close();
                if (file!=null)
                    file.Release();
                file = null;
            }
        }

     
    }
}