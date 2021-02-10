using BakedFileService.Protos;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using NeoCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BakedFileService
{
    // Remember that conceivable the Mover can move a file out from under us
    // We can open files on *any* filesystem, but really we'd rather stay with
    // files on our own

    // We'll use errors to invalidate our cache


    public class BakedVolumeService : BakedVolumeData.BakedVolumeDataBase
    {
        IMemoryCache openFileCache;
        private IMongoCollection<BakedVolumes> bvs;
        private readonly ILogger<BakedVolumeService> _logger;
        // Supposedly protobuf isn't so efficient after 1mb -- we'll test
        public const int BUFSIZ= 4096*1024;   // 4MiB
        byte[] buffer = new byte[BUFSIZ];

        public BakedVolumeService(ILogger<BakedVolumeService> logger, IMemoryCache cache)
        {
            _logger = logger;
            openFileCache = cache;
        }

        public override Task<FetchPayload> Fetch(FetchRequest request, ServerCallContext context)
        {
            return Task.FromResult(GetRecord(request));
        }

        public FetchPayload GetRecord(FetchRequest request)
        {
            var volume = request.BakedVolume;
            var part = request.Part;

         

            if (bvs == null)
                bvs = NeoMongo.NeoDb.BakedVolumes();

            var cacheKey = volume + "/" + part;
            VolCacheRec cache;
            if (!openFileCache.TryGetValue<VolCacheRec>(cacheKey, out cache))
                           cache = new VolCacheRec(bvs, volume, part, _logger);
            
            openFileCache.Set<VolCacheRec>(cacheKey, cache);

            // Our caches work, but we're not really managing any kind of residency.
            // We'll play with this and see how they work, but it's trivial to
            // simply DDOS the cache into insanity.

            // We're also not handling error retry, though that may not be our job.
            // We may have to make some specific errors flow back so the client can
            // deal with a moved file or a service down.   It theoretically can even
            // use the X file if one of the main 4 is not available to regen the
            // data flow with XOR (in degraded mode).   This service will read any part.

            var retRec = new FetchPayload();
            retRec.Error = String.Empty;

            if (cache.VolumeStream==null)
                retRec = cache.OpenStream();
            if (retRec.Error != String.Empty)
            {
                Console.WriteLine($"? Encountered error: {retRec.Error}");
                return retRec;
            }

            // Now if the file is gone/moved we might get an error seeking

            var didSeek = cache.VolumeStream.Seek(request.Offset, SeekOrigin.Begin);

            var length = Math.Min(BUFSIZ, request.RequestCount);
            var count = cache.VolumeStream.Read(buffer, 0, length);



            retRec.Length = count;  // Not sure what's efficient here - nice if we didn't have to copy
            retRec.Payload = ByteString.CopyFrom(buffer, 0, count);

            Console.WriteLine($"[Read {count} bytes @{request.Offset} offset (actual={didSeek}, newpos={cache.VolumeStream.Position})");

            return retRec;
        }

        internal class VolCacheRec : IDisposable
        {
            private string fName;
            private BinaryReader binOpen;

            public Stream VolumeStream { get; private set; }

            public IMongoCollection<BakedVolumes> Bvs { get; private set; }
            public string Volume { get; }
            public string Part { get; }

            ILogger<BakedVolumeService> logger;
            private bool disposedValue;
            public BakedVolumes VolumeRec { get; private set; }

            public VolCacheRec(IMongoCollection<BakedVolumes> bvs, string volume, string part, ILogger<BakedVolumeService> _logger)
            {
                Bvs = bvs;
                Volume = volume;
                Part = part;

                logger = _logger;

                Console.WriteLine($"[Setup cache for {Volume}/{Part}]");
            }

            public FetchPayload OpenStream()
            {
                var retRec = new FetchPayload();

                var filter = Builders<BakedVolumes>.Filter.Eq("_id", Volume);

                VolumeRec = Bvs.FindSync(filter).FirstOrDefault();
                if (VolumeRec == null)
                {
                    retRec.Error = $"Volume: {Volume} does not exist";
                    return retRec;
                }

                if (!VolumeRec.Parts.ContainsKey(Part))
                {
                    retRec.Error = $"Volume: {Volume} Part {Part} does not exist";
                    return retRec;
                }

                var p = VolumeRec.Parts[Part];

                fName = Path.Combine(p.Path, p.Name);

                try
                {
                    binOpen = new BinaryReader(File.Open(fName, FileMode.Open, FileAccess.Read));
                    VolumeStream = binOpen.BaseStream;
                }
                catch (Exception ex)
                {
                    retRec.Error = ex.Message;
                    if (logger != null)
                        logger.LogError(ex, $"Cannot open file {fName} for {Volume}/{Part}");
                    return retRec;
                }

                Console.WriteLine($"[Open Volume: {fName}]");

                return retRec;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (binOpen != null)
                            binOpen.Close();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                    // TODO: set large fields to null
                    VolumeStream = null;
                    binOpen = null;
                    Bvs = null;

                    disposedValue = true;
                }
            }

            // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
            // ~VolCacheRec()
            // {
            //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
