using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NeoScry;

public static class HashChecks
{
    public static void GetHashes(ScryContext context, bool justSha1)
    {
        // May not have looked up the asset yet (we'll use this to figure out if we need to)
        NeoAssets.Mongo.Assets_Hashes hashes = null;
        if (context.Asset != null) hashes = context.Asset.Hashes;

        var hashNeed = new List<HashAlgorithmName>();

        if (justSha1)
            hashNeed.Add(HashAlgorithmName.SHA1);
        else
        {
            if (hashes == null)
                throw new ArgumentException("asset is null and justSha1=true");

            // Don't recompute SHA1 if we already have it
            if (context.Sha1Key != null || context.Sha1Key.Length<1)
                hashes.SHA1 = Convert.ToHexString(context.Sha1Key);

            if (String.IsNullOrWhiteSpace(hashes.SHA1))
                hashNeed.Add(HashAlgorithmName.SHA1);
            if (String.IsNullOrWhiteSpace(hashes.MD5))
                hashNeed.Add(HashAlgorithmName.MD5);
            if (String.IsNullOrWhiteSpace(hashes.SHA256))
                hashNeed.Add(HashAlgorithmName.SHA256);
            if (hashes.SHA384 == null || hashes.SHA384.Length != 384 / 8)
                hashNeed.Add(HashAlgorithmName.SHA384);
            if (hashes.SHA512 == null || hashes.SHA512.Length != 512 / 8)
                hashNeed.Add(HashAlgorithmName.SHA512);
        }

        // WHIRLPOOL and TIGER are legacy from hashdeep for now

        var hashCompute = new List<HashAlgorithm>();

        foreach (var h in hashNeed)
            hashCompute.Add(HashAlgorithm.Create(h.Name));

        // See if we really need to do anything
        if (hashCompute.Count > 0)
            using (FileStream fileStream = File.OpenRead(context.PhysicalName))
            {
                var buffer_size = 0x4096;
                var buffer = new byte[buffer_size];

                int bytes_read;
                while ((bytes_read = fileStream.Read(buffer, 0, buffer.Length)) == buffer_size)
                {
                    foreach (var h in hashCompute)
                        h.TransformBlock(buffer, 0, bytes_read, buffer, 0);
                }

                // Compute all of the needed hashes
                foreach (var h in hashCompute)
                {
                    h.TransformFinalBlock(buffer, 0, bytes_read);

                    // Some of these are text and some are binary for legacy reasons
                    switch (h)
                    {
                        case SHA1 s:
                            context.Sha1Key = s.Hash;
                            context.Sha1Hex = Convert.ToHexString(s.Hash);

                            if (hashes != null)
                                hashes.SHA1 = context.Sha1Hex;
                            break;
                        case MD5 m:
                            if (hashes != null)
                                hashes.MD5 = Convert.ToHexString(m.Hash);
                            break;
                        case SHA256 s:
                            if (hashes != null)
                                hashes.SHA256 = Convert.ToHexString(s.Hash);
                            break;
                        case SHA384 s:
                            if (hashes != null)
                                hashes.SHA384 = s.Hash;
                            break;
                        case SHA512 s:
                            if (hashes != null)
                                hashes.SHA512 = s.Hash;
                            break;
                    }
                }

            }
    }
}

