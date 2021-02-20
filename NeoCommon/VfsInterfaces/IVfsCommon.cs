using System;
using Tmds.Linux;

namespace NeoCommon.VfsInterfaces
{
    /// <summary>
    /// Methods common between IVfsDirectoryInfo and IVfsFileInfo (things which could happen equally to a file or directory)
    /// </summary>
    public interface IVfsCommon
    {
        IVfsPath Path { get; set; }
        stat stat { get; set; }

        bool FileSystemReadOnly { get; set; }

        // Common API elements (between directory and file)

        int GetAttr(ref stat stat);
        int Chown(uint uid, uint gid);
        int Rename(IVfsPath newPath, int flags);
        int SymLink(IVfsPath target);
        int Unlink();
        int ReadLink(Span<byte> buffer);
        int Link(IVfsPath toPath);
        int UpdateTimestamps(ref timespec atime, ref timespec mtime);
        int Flush(ref IVfsCommon fi);
        int FSync(ref IVfsCommon fi);
        int GetXAttr(ReadOnlySpan<byte> name, Span<byte> data);
        int ListXAttr(Span<byte> list);
        int RemoveXAttr(ReadOnlySpan<byte> name);
        int Access(mode_t mode);
        int FAllocate(int mode, ulong offset, long length);


    }
}