using NeoBakedVolumes;
using NeoCommon.VfsInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Fuse;
using Tmds.Linux;

namespace Linux_FuseFilesystem
{
    class Fuse_VfsProvider : FuseFileSystemBase
    {
        IVfsFilesystem fs;

        // Take the virtual filesystem and provide all of the available functions for it
        public Fuse_VfsProvider(IVfsFilesystem fileSystem)
        {
            fs = fileSystem;
        }

        // Directory Functions
        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var di = fs.DirectoryInfo(new VfsLinuxPath(path));
      
            return base.OpenDir(path, ref fi);
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            return base.ReadDir(path, offset, flags, content, ref fi);
        }
        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            return base.GetAttr(path, ref stat, fiRef);
        }
        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            return base.MkDir(path, mode);
        }
        public override int RmDir(ReadOnlySpan<byte> path)
        {
            return base.RmDir(path);
        }
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            return base.Unlink(path);
        }
        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            return base.Rename(path, newPath, flags);
        }
        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            return base.ChMod(path, mode, fiRef);
        }
        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            return base.Chown(path, uid, gid, fiRef);
        }
        public override int FSyncDir(ReadOnlySpan<byte> readOnlySpan, bool onlyData, ref FuseFileInfo fi)
        {
            return base.FSyncDir(readOnlySpan, onlyData, ref fi);
        }
       
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.ReleaseDir(path, ref fi);
        }
        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            return base.UpdateTimestamps(path, ref atime, ref mtime, fiRef);
        }

        // Simple read file functions
        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.Open(path, ref fi);
        }
        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            return base.Read(path, offset, buffer, ref fi);
        }
        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            base.Release(path, ref fi);
        }
        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.Flush(path, ref fi);
        }
        // File Write operations
        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            return base.Create(path, mode, ref fi);
        }
        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi)
        {
            return base.Write(path, off, span, ref fi);
        }
        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            return base.Truncate(path, length, fiRef);
        }
        // Symbolic link operations
        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
        {
            return base.Link(fromPath, toPath);
        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer)
        {
            return base.ReadLink(path, buffer);
        }
        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target)
        {
            return base.SymLink(path, target);
        }
        // Attribute operations
        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            return base.GetXAttr(path, name, data);
        }
        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            return base.ListXAttr(path, list);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            return base.SetXAttr(path, name, data, flags);
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            return base.RemoveXAttr(path, name);
        }
        // Misc (I'm not sure what these are yet)
        public override int Access(ReadOnlySpan<byte> path, mode_t mode)
        {
            return base.Access(path, mode);
        }
        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            return base.FAllocate(path, mode, offset, length, ref fi);
        }
        public override int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.FSync(path, ref fi);
        }
       
        // Filesystem level operations
        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            return base.StatFS(path, ref statfs);
        }
    }
}
