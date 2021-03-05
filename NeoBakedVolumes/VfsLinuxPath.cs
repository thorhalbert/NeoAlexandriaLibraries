using NeoCommon.VfsInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoBakedVolumes
{
    public unsafe class VfsLinuxPath : IVfsPath
    {
        private byte[] path;

        public VfsLinuxPath(ReadOnlySpan<byte> rawPath)
        {
             path = rawPath.ToArray();   // For now
        }

        public unsafe byte* toNullTerm()
        {
            var l = path.Length;
            var newP = new byte[l + 1];
            Array.Copy(path, newP, l);
            newP[l] = 0;  // Put the terminator on it

            fixed (byte* p = newP)
                return p;
        }
    }
}
