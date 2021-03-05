using NeoCommon.VfsInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Linux;

namespace NeoBakedVolumes
{
    /// <summary>
    /// Give the libc calls palletable signatures -- let's push all the unsafe stuff into
    /// here (this might become it's own assembly eventually).
    /// </summary>
    internal unsafe static class Wrap_LibC
    {
        public static int Open(IVfsPath path, int flags, mode_t mode= default )
        {
            var fd = LibC.open(path.toNullTerm(), 0);
            if (LibC.errno != 0)
                return -LibC.errno;
            return fd;
        }

        internal static int Read(int fd, ulong offset, Span<byte> buffer)
        {
            if (fd == -1) return LibC.EACCES;

            LibC.lseek(fd, (long) offset, LibC.SEEK_SET);
            if (LibC.errno != 0)
                return -LibC.errno;

            ssize_t rd;
            fixed (void* bp = buffer)
            {
                rd = LibC.read(fd, bp, buffer.Length);
            }
            if (rd >= 0) return (int) rd;

            return -LibC.errno;
        }
    }

}
