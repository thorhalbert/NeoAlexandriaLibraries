using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.Linux;

namespace PenguinSanitizer
{
    public static class Extensions
    {
        public static Span<byte> ToSpan(this string str)
        {
            return Encoding.ASCII.GetBytes(str).AsSpan();
        }

        public static string GetString(this ReadOnlySpan<byte> path)
        {
            return Encoding.UTF8.GetString(path.ToArray());
        }
        public static string GetString(this ReadOnlyMemory<byte> path)
        {
            return Encoding.UTF8.GetString(path.ToArray());
        }
        public static string GetString(this byte[] path)
        {
            return Encoding.UTF8.GetString(path);
        }

        public static unsafe byte* ToBytePtr(this byte[] path)
        {
            var l = path.Length;
            var newP = new byte[l + 1];

            Array.Copy(path, newP, l);
            newP[l] = 0;  // Put the terminator on it

            fixed (byte* p = newP)
                return p;
        }
        public static unsafe byte* ToBytePtr(this ReadOnlySpan<byte> path)
        {
            return ToBytePtr(path.ToArray());
        }
        public static unsafe byte* ToBytePtr(this Span<byte> path)
        {
            return ToBytePtr(path.ToArray());
        }
        public static unsafe byte* ToBytePtr(this IEnumerable<byte> path)
        {
            return ToBytePtr(path.ToArray());
        }

        // Find end of zero term string and return it as a byte[] with \0 stripped
        public static byte[] ZeroTermString(this byte[] inbuf, bool RemoveNull = true)
        {
            int rem = 1;
            if (!RemoveNull) rem = 0;
            for (var j = 0; j < inbuf.Length; j++) // Don't have .index
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

        public static DateTimeOffset ToDTO(this timespec spec)
        {
            var s = spec.tv_sec;
            var n = spec.tv_nsec;

            var dto = DateTimeOffset.FromUnixTimeSeconds(s);
            dto.AddMilliseconds(n / 1000.0);

            return dto;
        }

        // Almost same as above but return byte[] from IntPtr string
        //public static unsafe byte[] StringPtrToBytes(this IntPtr p)
        //{
        //    if (p == IntPtr.Zero)
        //        return null;

        //    int len = checked((int) Mono.Unix.Native.Stdlib.strlen(p));

        //    var ret = new byte[len];
        //    Marshal.Copy(p, ret, 0, len);

        //    return ret;
        //}
        //public static unsafe IntPtr ToNullTerm(this byte[] path)
        //{
        //    var l = path.Length;
        //    var newP = new byte[l + 1];

        //    Array.Copy(path, newP, l);
        //    newP[l] = 0;  // Put the terminator on it

        //    fixed (byte* p = newP)
        //        return new IntPtr(p);
        //}     
    }
    public class ByteMemoryComp : IComparer<ReadOnlyMemory<byte>>
    {
        public int Compare(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
        {
            return x.Span.SequenceCompareTo(y.Span);
        }
    }
}
