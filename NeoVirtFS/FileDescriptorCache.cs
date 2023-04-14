using NeoAssets.Mongo;
using NeoCommon;
using System.Text;
using Tmds.Linux;

namespace NeoVirtFS
{
    internal class FileDescriptorCache : FileDescriptor, INeoVirtFile
    {
        private int iot=0;

        public bool Dirty { get; set; } = false;

        public FileDescriptorCache(NeoAssets.Mongo.NeoVirtFS myFile) : base(myFile)
        {
        }


       // private IEnumerator<string> pathStr(string )
            
        public unsafe int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            Console.WriteLine($"[Cache-Create {Encoding.ASCII.GetString(fds.FileNode.Content.CacheFile)}]");

            var fileg = fds.FileNode.Content.CachePoolGuid.Value.ToString().ToLower();

            // Intermediate directories probably don't exist - this needs to be much smarter
            var ca1 = $"/ua/NeoVirtCache/{fileg.Substring(0, 3)}";
            //var ca2 = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}/{fileg.Substring(2, 2)}";
            //var ca3 = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}/{fileg.Substring(2, 2)}/{fileg.Substring(4, 2)}";
            //var ca4 = $"/ua/NeoVirtCache/{fileg.Substring(0, 2)}/{fileg.Substring(2, 2)}/{fileg.Substring(4, 2)}/{fileg.Substring(6, 2)}/";

            // Need to extract the path (CacheFile) and do a mkdir -p within it (not assuming we know what the path here in ca1)

            var ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca1)), 0b111_101_000);
            //ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca2)), 0b111_101_000);
            //ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca3)), 0b111_101_000);
            //ret = LibC.mkdir(toBp(Encoding.ASCII.GetBytes(ca4)), 0b111_101_000);

            iot = LibC.open(toBp(fds.FileNode.Content.CacheFile), flags, 0b110_100_000);
            if (iot < 0)
                return iot;

            Dirty = true;

            return 0;
        }

        public int Open(FileDescriptor fds, int flags)
        {
            // Open existing file for read or write


            iot = base.Open(fds.FileNode.Content.CacheFile, flags);
            if (iot < 0)
                return iot;


            // This could dirty our file - we might rely on 'write' below, but we should try

            var accmode = flags & LibC.O_ACCMODE;

            if (accmode == LibC.O_WRONLY)
                Dirty = true;
            //else if (accmode == LibC.O_RDWR)
            //    mode = "O_RDWR";

            if ((flags & LibC.O_CREAT) != 0)
                Dirty = true;
            //if ((flags & LibC.O_EXCL) != 0)
            //    mode += ", O_EXCL";
            //if ((flags & LibC.O_NOCTTY) != 0)
            //    mode += ", O_NOCTTY";
            if ((flags & LibC.O_TRUNC) != 0)
                Dirty = true;
            //if ((flags & LibC.O_APPEND) != 0)
              
            //if ((flags & LibC.O_NONBLOCK) != 0)
            //    mode += ", O_NONBLOCK";
            //if ((flags & LibC.O_DSYNC) != 0)
            //    mode += ", O_DSYNC";
            //if ((flags & LibC.FASYNC) != 0)
            //    mode += ", FASYNC";
            //if ((flags & LibC.O_DIRECT) != 0)
            //    mode += ", O_DIRECT";
            //if ((flags & LibC.O_LARGEFILE) != 0)
            //    mode += ", O_LARGEFILE";
            //if ((flags & LibC.O_DIRECTORY) != 0)
            //    mode += ", O_DIRECTORY";
            //if ((flags & LibC.O_NOFOLLOW) != 0)
            //    mode += ", O_NOFOLLOW";
            //if ((flags & LibC.O_NOATIME) != 0)
            //    mode += ", O_NOATIME";
            //if ((flags & LibC.O_CLOEXEC) != 0)
            //    mode += ", O_CLOEXEC";

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

            // By here we have created the file and closed it - need to persist that it exists or update record that it's closed

            return 0;
        }

        public int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span)
        {
            // File is dirtied, hash is changing

            Dirty = true;

            return base.Write(iot, off, span);
        }

        public unsafe new byte* toBp(ReadOnlySpan<byte> path)
        {
            return RawDirs.ToBytePtr(path.ToArray());
        }
    }

}
