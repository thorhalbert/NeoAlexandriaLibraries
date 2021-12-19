using NeoCommon;
using PenguinSanitizer;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Linux;
using System.Linq;
using System.Security.Cryptography;
using Tmds.Fuse;

namespace NeoScry
{
    public static class ScanFileDirectory
    {
        public static readonly byte[] pathSep = new byte[] { (byte) '/' };
        public static readonly byte[] dot = new byte[] { (byte) '.' };
        public static readonly byte[] dotdot = new byte[] { (byte) '.', (byte) '.' };

        public static List<ReadOnlyMemory<byte>> Scan(ReadOnlySpan<byte> path)
        {
            Console.WriteLine($"Scan: {path.GetString()}");
            var dirFd = RawDirs.opendir(RawDirs.ToNullTerm(path.ToArray()));
            if (dirFd == IntPtr.Zero)
                throw new Exception($"Error opening directory {path.GetString()} - {LibC.errno}");

            var contents = new List <ReadOnlyMemory<byte>>();

            while (true)
            {
                var dir = RawDirs.readdir_wrap(dirFd, false);  // Don't remove the null
                if (dir == null) break;

                var d = dir.Value;

                //Console.WriteLine($"See {d.d_name.GetString()}");

                contents.Add(d.d_name.AsMemory<byte>());                             
            }

            RawDirs.closedir(dirFd);

            return contents;
        }

        public static FileNode RecursiveScan(ReadOnlySpan<byte> startingPath, ReadOnlySpan<byte> name, int recurseGuard=0)
        {
            var fn = new FileNode();
            if (recurseGuard > 1000) return fn;

            fn.Load(startingPath, name);
            if (!fn.IsDir) return fn;   // We've done our duty 

            var files = Scan(startingPath);

            // Sort it
            var cmp = new ByteMemoryComp();
            files.Sort(comparer: cmp);

            // Assimilate the children
            foreach (var f in files)
            {
                if (f.Span.SequenceEqual(dot)) continue;
                if (f.Span.SequenceEqual(dotdot)) continue;

                // This surely sucks - got to be better ways to do this. -- Need bytebuilder
                var newPath = startingPath.ToArray().Concat(pathSep.ToArray()).Concat(f.ToArray()).ToArray();

                //Console.WriteLine($"See {newPath.GetString()}");

                var newNode = RecursiveScan(newPath, f.Span, recurseGuard+1);

                fn.Members.Add(newNode);
            }

            return fn;
        }

        public unsafe static int Stat(ReadOnlySpan<byte> path, ref stat newStat)
        {
            fixed (stat* ns = &newStat)
            {
                var lc = LibC.lstat(path.ToBytePtr(), ns);
                if (lc < 0)
                    return LibC.errno;
            }
            return 0;
        }


        public unsafe static byte[] GetSha1(byte[] path)
        {
            var ps = PenguinSanitizer.Extensions.ToBytePtr(path);
            var newFd = LibC.open(ps, LibC.O_RDONLY);

            // Tap the stream so we can get a hash on it.

            var hash = HashAlgorithm.Create("SHA1");
         
            var buf = new Byte[32 * 1024 * 1024];

            while (true)
            {
                ssize_t rd = 0;
                fixed (void* b = buf)
                {
                    rd = LibC.read(newFd, b, buf.Length);

                    hash.TransformBlock(buf, 0, (int) rd, buf, 0);                  
                }
    
                if (rd < 1)
                    break;
            }

            LibC.close(newFd);
            buf = null;

            hash.TransformFinalBlock(buf, 0, 0);
            return hash.Hash;
        }
    }

    public struct FileNode   // ref struct is probalby to extreme here
    {
        private stat fileStat;

        public ReadOnlyMemory<byte> Name { get; internal set; }
        public stat FileStat { get => fileStat; internal set => fileStat = value; }
        public ReadOnlyMemory<byte> SymbolicLink { get; internal set; }

        public List<FileNode> Members { get; } = new List<FileNode>();

        public byte[] AssetSHA1 { get; set; }

        internal unsafe void Load(ReadOnlySpan<byte> startingPath, ReadOnlySpan<byte> name)
        {
            Name = name.ToArray();

            var ret = ScanFileDirectory.Stat(startingPath, ref fileStat);
            if (ret != 0) throw new LinuxError(ret);

            if (!IsLnk) return;

            ssize_t retl;
            var buffer = new byte[1024];
            fixed (byte* b = buffer)
            {
                retl = LibC.readlink(startingPath.ToBytePtr(), b, buffer.Length);
            }

            SymbolicLink = MemoryExtensions.AsMemory<byte>(buffer, 0, (int) retl);        
        }

        public bool IsDir { get { return (FileStat.st_mode & LibC.S_IFMT) == LibC.S_IFDIR; } }
        public bool IsFile { get { return (FileStat.st_mode & LibC.S_IFMT) == LibC.S_IFREG; } }
        public bool IsLnk { get { return (FileStat.st_mode & LibC.S_IFMT) == LibC.S_IFLNK; } }
    }
}
