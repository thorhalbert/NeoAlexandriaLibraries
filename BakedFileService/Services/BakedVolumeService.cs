﻿using BakedFileService.Protos;
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
        private const int BUFSIZ= 1024*1024*1024;
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

            var attempts = 0;


            var retRec = new FetchPayload();

            if (cache.VolumeStream==null)
                cache.OpenStream();
            if (retRec.Error != null)
                return retRec;

            // Now if the file is gone/moved we might get an error seeking

            cache.VolumeStream.Seek(request.Offset, SeekOrigin.Begin);
            var length = Math.Min(BUFSIZ, request.RequestCount);
            var count = cache.VolumeStream.Read(buffer, 0, length);

            retRec.Length = count;  // Not sure what's efficient here - nice if we didn't have to copy
            retRec.Payload = ByteString.CopyFrom(buffer, 0, count);

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
