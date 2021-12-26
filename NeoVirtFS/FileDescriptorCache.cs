using NeoAssets.Mongo;
using Tmds.Linux;

namespace NeoVirtFS
{
    internal class FileDescriptorCache : FileDescriptor, INeoVirtFile
    {
        private int iot=0;

        public FileDescriptorCache(NeoAssets.Mongo.NeoVirtFS myFile) : base(myFile)
        {
        }

        public int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            iot = base.Create(fds.FileNode.Content.CacheFile, 0b110_100_000, flags);
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
    }

}
