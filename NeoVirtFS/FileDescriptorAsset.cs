using NeoAssets.Mongo;
using Tmds.Linux;

namespace NeoVirtFS
{
    internal class FileDescriptorAsset : FileDescriptor, INeoVirtFile
    {
        public FileDescriptorAsset(NeoAssets.Mongo.NeoVirtFS myFile) : base(myFile)
        {
        }

        public int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            throw new NotImplementedException();
        }

        public int Open(FileDescriptor fds, int flags)
        {
            throw new NotImplementedException();
        }

        public int Read(FileDescriptor fds, ulong offset, Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public int Release(FileDescriptor fds)
        {
            throw new NotImplementedException();
        }

        public int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span)
        {
            throw new NotImplementedException();
        }

     
    }

}
