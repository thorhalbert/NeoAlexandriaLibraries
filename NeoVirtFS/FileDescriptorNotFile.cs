using NeoAssets.Mongo;
using Tmds.Linux;

namespace NeoVirtFS
{
    internal class FileDescriptorNotFile : FileDescriptor, INeoVirtFile
    {
        public FileDescriptorNotFile(NeoAssets.Mongo.NeoVirtFS myFile) : base(myFile)
        {
        }

        public int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            return -LibC.EPERM;   // Can't create NotFile
        }

        public int Open(FileDescriptor fds, int flags)
        {
            return 0;   // Open always succeeds
        }

        public int Read(FileDescriptor fds, ulong offset, Span<byte> buffer)
        {
            return 0;   // Read always gets eof
        }

        public int Release(FileDescriptor fds)
        {
            return 0;   // Close always suceeds
        }

        public int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span)
        {
            return -LibC.EPERM;  // Can't write notfile
        }
    }

}
