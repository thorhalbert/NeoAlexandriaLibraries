using NeoAssets.Mongo;
using NeoCommon;
using System.Text;
using Tmds.Linux;

namespace NeoVirtFS
{
    internal class FileDescriptorCache : FileDescriptor, INeoVirtFile
    {
        private int iot=0;

        public FileDescriptorCache(NeoAssets.Mongo.NeoVirtFS myFile) : base(myFile)
        {
        }


       // private IEnumerator<string> pathStr(string )
            
        public unsafe int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            Console.WriteLine($"[Cache-Create {Encoding.ASCII.GetString(fds.FileNode.Content.CacheFile)}]");

            var fileg = fds.FileNode.Content.CachePoolGuid.Value.ToString().ToLower();


            // Intermediate directories probably don't exist - this needs to be much smarter
            var ca1 = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}";
            var ca2 = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}/{fileg.Substring(2, 2)}";
            var ca3 = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}/{fileg.Substring(2, 2)}/{fileg.Substring(4, 2)}";
            var ca4 = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}/{fileg.Substring(2, 2)}/{fileg.Substring(4, 2)}/{fileg.Substring(6, 2)}/";

            var ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca1)), 0b111_101_000);
            ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca2)), 0b111_101_000);
            ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca3)), 0b111_101_000);
            ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca4)), 0b111_101_000);

            iot = LibC.open(toBp(fds.FileNode.Content.CacheFile), flags, 0b110_100_000);
            if (iot < 0)
                return iot;

            return 0;
        }

        public int Open(FileDescriptor fds, int flags)
        {
            iot = base.Open(fds.FileNode.Content.CacheFile, flags);
            if (iot < 0)
                return iot;

            return 0;
        }

        public int Read(FileDescriptor fds, ulong offset, Span<byte> buffer)
        {
            return base.Read(iot, offset, buffer);
        }

        public int Release(FileDescriptor fds)
        {
            base.Release(iot);
            iot = 0;

            return 0;
        }

        public int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span)
        {
            return base.Write(iot, off, span);
        }

        public unsafe byte* toBp(ReadOnlySpan<byte> path)
        {
            return RawDirs.ToBytePtr(path.ToArray());
        }
    }

}
