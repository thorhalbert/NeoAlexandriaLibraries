using NeoAssets.Mongo;
using Tmds.Linux;

namespace NeoVirtFS
{
    internal class FileDescriptorPhysical : FileDescriptor, INeoVirtFile
    {
        int iot = 0;

        public FileDescriptorPhysical(NeoAssets.Mongo.NeoVirtFS myFile) : base(myFile)
        {
        }

        public int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            return -LibC.EPERM;   // Can't create physicalfile
           
        }

        public int Open(FileDescriptor fds, int flags)
        {
            iot = base.Open(fds.FileNode.Content.PhysicalFile, flags);
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
            return -LibC.EPERM;  // Can't write physicalfile
        }
    }

}
