using BakedFileService.Protos;
using Grpc.Net.Client;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.IO;
using Grpc.Net.Client.Configuration;
using Grpc.Core;

namespace AssetFileSystem
{
    public partial class AssetFile
    {
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

            public ReadOnlyMemory<byte> ReadNextBlockWithGRPC(uint size = 0)
            {
                var defaultMethodConfig = new MethodConfig
                {
                    Names = { MethodName.Default },
                    RetryPolicy = new RetryPolicy
                    {
                        MaxAttempts = 16,
                        InitialBackoff = TimeSpan.FromSeconds(1),
                        MaxBackoff = TimeSpan.FromSeconds(5),
                        BackoffMultiplier = 1.5,
                        RetryableStatusCodes = { StatusCode.Unavailable }
                    }
                };

                // Figure out what system our primary connector is on
                var channel = GrpcChannel.ForAddress("http://feanor:5000",
                                    new GrpcChannelOptions
                                    {
                                        MaxReceiveMessageSize = null, // 5 MB
                                        MaxSendMessageSize = null, // 2 MB
                                        //HttpHandler = new System.Net.Http.SocketsHttpHandler
                                        //{
                                        //    EnableMultipleHttp2Connections = true,
                                        //}
                                        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } }
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

                    Console.WriteLine($"[Proceed to next file {FileNum}]");

                    offsetx = 0;   // Start at beginning of next file

                    currentPartRemain = Math.Min(volumelength, realLength - fileRead);
                }

                return returnVal.Payload.Memory.Slice(0, length);
            }

            Memory<byte> mainBuff = new Memory<byte>(new byte[0]);

            public ReadOnlyMemory<byte> ReadNextBlockDirect(uint size = 0)
            {
                if (fileRemain < 1)
                    return new ReadOnlyMemory<byte>(); //EOF

                if (size < 1)
                    size = BUFFSIZE;

                if (mainBuff.Length < 1)
                    mainBuff = new Memory<byte>(new byte[size]);


                if (FileNum != currentFileNum)
                {
                    // Close the old file, which we don't really have to do -- the service cache does that

                    // Open the new, which again the service does, we just need to say what
                    currentFileNum = FileNum;
                    currentPart = Convert.ToChar(currentFileNum + 65).ToString();  // File 0=A
                }

                if (size > fileRemain)
                    size = (uint) fileRemain;

                // Need to lazy load the filename -- need to look up and open (close previous if we had it open)

                OpenStream(volRec._id, currentPart);

                // These files are guaranteed to be under 32 bits in length
                //var returnVal = assetClient.Fetch(new FetchRequest()
                //{
                //    BakedVolume = volRec._id,
                //    Part = currentPart,
                //    Offset = (int) offsetx,
                //    RequestCount = (int) size,
                //});

                _VolumeStream.Seek((long) offsetx, SeekOrigin.Begin);

                var readLength = _VolumeStream.Read(mainBuff.Span);

                var length = readLength;
                fileRead += (ulong) length;

                //var dump = Utils.HexDump(returnVal.Payload.ToByteArray());
                //Console.WriteLine(dump);

                Console.WriteLine($"Read: Part {currentFileNum}, Length {length} Read {fileRead} Remain {currentPartRemain} ");

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

                return mainBuff.Slice(0, length);
            }

            internal void Release()
            {
                // Let our GC happen quickly

                // If we have files open (in direct mode -- need to release them)

                //if (_VolumeFile != null)
                //    Console.WriteLine($"Release file: " + _VolumeFile);

                if (_VolumeStream != null)
                    _VolumeStream.Close();

                if (_VolumeBase != null)
                    _VolumeBase.Close();

                _VolumeStream = null;
                _VolumeBase = null;
                _VolumeFile = null;
            }


            string _VolumeFile = null;
            BinaryReader _VolumeBase = null;
            Stream _VolumeStream=null;
            public void OpenStream(string volume, string part)
            {
                // See if we need to switch part files
                var vf = $"{volume}/{part}";
                if (_VolumeFile == vf) return;
     
               

                var filter = Builders<BakedVolumes>.Filter.Eq("_id", volume);

                var VolumeRec = volRec;

                if (!VolumeRec.Parts.ContainsKey(part))
                {
                    throw new ArgumentException($"Volume: {volume} Part {part} does not exist");
                   
                }

                var p = VolumeRec.Parts[part];

                var fName = Path.Combine(p.Path, p.Name);

                try
                {
                    _VolumeBase = new BinaryReader(System.IO.File.Open(fName, FileMode.Open, FileAccess.Read, FileShare.Read));
                 
                    _VolumeStream = _VolumeBase.BaseStream;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Open file Error: {ex.Message} {ex.StackTrace}");
                    throw new Exception( $"Error Opening file - {fName} for {volume}/{part}",ex);
                }

                Console.WriteLine($"[Open Volume: {fName}]");
                _VolumeFile = vf;
            }
        }

     
    }
}