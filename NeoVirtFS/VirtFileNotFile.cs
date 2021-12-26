using NeoAssets.Mongo;

namespace NeoVirtFS
{
    internal class FileDescriptorNotFile : FileDescriptor
    {
        public FileDescriptorNotFile(NeoVirtFSContent Content, INeoVirtFile Handler, bool Create) : base(Content, Handler, Create)
        {
        }
    }
    internal class VirtFileNotFile : INeoVirtFile
    {
      
        public VirtFileNotFile()
        {
        }

        public int Open(FileDescriptor fds)
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
