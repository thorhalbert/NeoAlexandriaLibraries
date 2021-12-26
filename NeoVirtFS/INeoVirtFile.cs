using NeoAssets.Mongo;
using Tmds.Linux;

namespace NeoVirtFS
{
    public interface INeoVirtFile
    {
        // This is a base class - different interfaces can store what's necessary
        int Create(FileDescriptor fds, mode_t mode, int flags);
        int Open(FileDescriptor fds, int flags);  // Get what you need
        int Read(FileDescriptor fds, ulong offset, Span<byte> buffer);
        int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span);
        int Release(FileDescriptor fds);    // Release private things - we'll free the FDS
    }
}
