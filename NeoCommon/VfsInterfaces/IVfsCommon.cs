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

        int GetAttr(IVfsPath path, ref stat stat, IVfsCommon fiRef);
        int Chown(IVfsPath path, uint uid, uint gid, IVfsCommon fiRef);
        int Rename(IVfsPath path, IVfsPath newPath, int flags);
        int SymLink(IVfsPath path, IVfsPath target);
        int Unlink(IVfsPath path);
        int ReadLink(IVfsPath path, Span<byte> buffer);
        int ChMod(IVfsPath path, mode_t mode, IVfsCommon fiRef);
        int Link(IVfsPath fromPath, IVfsPath toPath);
        int UpdateTimestamps(IVfsPath path, ref timespec atime, ref timespec mtime, IVfsCommon fiRef);
        int Flush(IVfsPath path, ref IVfsCommon fi);
        int FSync(IVfsPath path, ref IVfsCommon fi);
        int SetXAttr(IVfsPath path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags);
        int GetXAttr(IVfsPath path, ReadOnlySpan<byte> name, Span<byte> data);
        int ListXAttr(IVfsPath path, Span<byte> list);
        int RemoveXAttr(IVfsPath path, ReadOnlySpan<byte> name);
        int Access(IVfsPath path, mode_t mode);
        int FAllocate(IVfsPath path, int mode, ulong offset, long length, ref IVfsCommon fi);


    }
}