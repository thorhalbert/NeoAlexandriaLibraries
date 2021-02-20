using NeoCommon.VfsInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoBakedVolumes
{
    public class VfsLinuxPath : IVfsPath
    {
        private byte[] path;

        public VfsLinuxPath(ReadOnlySpan<byte> rawPath)
        {
             path = rawPath.ToArray();   // For now
        }

        
    }
}
