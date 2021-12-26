using NeoAssets.Mongo;

namespace NeoVirtFS
{
    public interface INeoVirtFile
    {
        // This is a base class - different interfaces can store what's necessary
       
        int Open(FileDescriptor fds);  // Get what you need
        int Read(FileDescriptor fds, ulong offset, Span<byte> buffer);
        int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span);
        int Release(FileDescriptor fds);    // Release private things - we'll free the FDS
    }
}
