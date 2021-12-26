using NeoAssets.Mongo;

namespace NeoVirtFS
{
    internal class FileDescriptorCache : FileDescriptor
    {
        public FileDescriptorCache(NeoVirtFSContent Content, INeoVirtFile Handler, bool Create) : base(Content, Handler, Create)
        {
        }
    }
    internal class VirtFileCache : INeoVirtFile
    {
        private NeoAssets.Mongo.NeoVirtFS myFile;

        public VirtFileCache(NeoAssets.Mongo.NeoVirtFS myFile)
        {
            this.myFile = myFile;
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
