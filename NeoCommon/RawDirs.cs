using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NeoCommon
{
    public static class RawDirs
    {
        internal const string LIBC = "libc";

        // Low level wrappers for the opendir calls we need

        // TMDS isn't providing us with the low level stuff (getdirent) to be able to write our own opendir
        // Mono.unix is giving us path's as strings which won't work either (linux filenames just aren't unicode strings)

        [DllImport(LIBC, SetLastError = true)]
        public static extern IntPtr opendir(IntPtr name);

        [DllImport(LIBC, SetLastError = true)]
        public static extern IntPtr closedir(IntPtr dir);

        // This are copied from the mono, but with the byte marshall

        public struct DirEnt
        {
            public ulong d_ino;
            public long d_off;
            public ushort d_reclen;
            public byte d_type;
          
            public byte[] d_name;
        }

        private struct _Dirent
        {
            public ulong d_ino;
            public long d_off;
            public ushort d_reclen;
            public byte d_type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]  // Not sure of this value (the 256)
            public byte[]  d_name;
        }

        private static object  _dir_lock = new object();

        [DllImport(LIBC, SetLastError = true)]
        private static extern IntPtr readdir(IntPtr dir);

        // Let's not let the user use the raw readdir - it is returning an unmanaged struct
        public static DirEnt? readdir_wrap(IntPtr dir, bool RemoveNull = true)
        {
            //_Dirent den;
            IntPtr ptr;
           
            lock (_dir_lock)
            {
                ptr  = readdir(dir);
            }

            if (ptr == IntPtr.Zero) return null;

            var den = Marshal.PtrToStructure<_Dirent>(ptr);

            var rt = new DirEnt()
            {
                d_ino = den.d_ino,
                d_off = den.d_off,
                d_reclen = den.d_reclen,
                d_type = den.d_type,
                d_name = ZeroTermString(den.d_name, RemoveNull)
            };

            return rt;
        }

        // Find end of zero term string and return it as a byte[] with \0 stripped
        public static byte[] ZeroTermString(byte[] inbuf, bool RemoveNull=true)
        {
            int rem = 1;
            if (!RemoveNull) rem = 0;
            for(var j=0; j<inbuf.Length; j++) // Don't have .index
            {
                if (inbuf[j] == 0)
                {
                    var outB = new byte[j - rem];
                    Array.Copy(inbuf, outB, j - rem);

                    return outB;
                }

            }
            return new byte[0];
        }

        // Almost same as above but return byte[] from IntPtr string
        public static unsafe byte[] StringPtrToBytes(IntPtr p)
        {
            if (p == IntPtr.Zero)
                return null;

            int len = checked((int) Mono.Unix.Native.Stdlib.strlen(p));

            var ret = new byte[len];
            Marshal.Copy(p, ret, 0, len);

            return ret;
        }
        public static unsafe IntPtr ToNullTerm(byte[] path)
        {
            var l = path.Length;
            var newP = new byte[l + 1];

            Array.Copy(path, newP, l);
            newP[l] = 0;  // Put the terminator on it

            fixed (byte* p = newP)
                return new IntPtr(p);
        }

        public static unsafe byte* ToBytePtr(byte[] path)
        {
            var l = path.Length;
            var newP = new byte[l + 1];

            Array.Copy(path, newP, l);
            newP[l] = 0;  // Put the terminator on it

            fixed (byte* p = newP)
                return p;
        }

        public static string HR(ReadOnlySpan<byte> path)
        {
            return Encoding.UTF8.GetString(path.ToArray());
        }
    }
}

