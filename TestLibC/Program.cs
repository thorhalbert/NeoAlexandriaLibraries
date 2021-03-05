using NeoCommon;
using System;
using System.Runtime.InteropServices;
using System.Text;
using Tmds.Linux;

namespace TestLibC
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            Console.WriteLine("Emulate opendir()");

            //var buf = new byte[65536];

            //var fd = LibC.open(toNullTerm(Encoding.UTF8.GetBytes("/tmp")), LibC.O_DIRECTORY|LibC.O_RDONLY);
            //Console.WriteLine($"Open fd={fd} errno={LibC.errno}");

            //LibC.fcntl(fd, LibC.F_SETFD, LibC.FD_CLOEXEC);
            //Console.WriteLine($"Fcntl fd={fd} errno={LibC.errno}");

            //var dh = Mono.Unix.Native.Syscall.opendir("/tmp");

            //while (true)
            //{
            //    var rd = Mono.Unix.Native.Syscall.readdir(dh);
            //    if (rd == null) break;
            //    Console.WriteLine($"{rd.d_name}");
            //}

            //fixed (byte* p = buf)
            //{
            //    var count = LibC.read(fd, p, 65536);
            //    Console.WriteLine($"Read {(int) count}");
            //    dumpBytes(buf, (int) count);
            //}

            var dh = RawDirs.opendir(toNullTerm(Encoding.UTF8.GetBytes("/tmp")));

            while (true)
            {
                var dir = RawDirs.readdir_wrap(dh);
                if (dir == null) break;

                var d = dir.Value;

                var name = Encoding.UTF8.GetString(d.d_name);
                Console.WriteLine($"{name}");
            }

            RawDirs.closedir(dh);
        }

        private static void dumpBytes(byte[] buffer, int count)
        {
            var len = count;

            // Dump 16 bytes per line
            for (int ix = 0; ix < len; ix += 16)
            {
                var cnt = Math.Min(16, len - ix);
                var line = new byte[cnt];
                Array.Copy(buffer, ix, line, 0, cnt);
                // Write address + hex + ascii
                Console.Write("{0:X6}  ", ix);
                Console.Write(BitConverter.ToString(line));
                Console.Write("  ");
                // Convert non-ascii characters to .
                for (int jx = 0; jx < cnt; ++jx)
                    if (line[jx] < 0x20 || line[jx] > 0x7f) line[jx] = (byte) '.';
                Console.WriteLine(Encoding.ASCII.GetString(line));
            }
        }
        public static unsafe IntPtr toNullTerm(byte[] path)
        {
            var l = path.Length;
            var newP = new byte[l + 1];
           
            Array.Copy(path, newP, l);
            newP[l] = 0;  // Put the terminator on it

            fixed (byte* p = newP)
                return new IntPtr(p);
        }
    }
}
