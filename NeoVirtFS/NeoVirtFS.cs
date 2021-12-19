using MongoDB.Driver;
using NeoAssets.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.Fuse;
using Tmds.Linux;

namespace NeoVirtFS
{
    public class NeoVirtFS : FuseFileSystemBase
    {
        IMongoCollection<NeoAssets.Mongo.NeoVirtFS> NeoVirtFSCol = null;
        public NeoVirtFS(IMongoDatabase db)
        {
            NeoVirtFSCol = db.NeoVirtFS();

        }
        public override int Access(ReadOnlySpan<byte> path, mode_t mode)
        {
            return base.Access(path, mode);
        }
        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            return base.ChMod(path, mode, fiRef);
        }
        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            return base.Chown(path, uid, gid, fiRef);
        }
        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            return base.Create(path, mode, ref fi);
        }
        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            return base.FAllocate(path, mode, offset, length, ref fi);
        }
        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.Flush(path, ref fi);
        }
        public override int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.FSync(path, ref fi);
        }
        public override int FSyncDir(ReadOnlySpan<byte> readOnlySpan, bool onlyData, ref FuseFileInfo fi)
        {
            return base.FSyncDir(readOnlySpan, onlyData, ref fi);
        }
        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            return base.GetAttr(path, ref stat, fiRef);
        }
        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            return base.GetXAttr(path, name, data);
        }
        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
        {
            return base.Link(fromPath, toPath);
        }
        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            return base.ListXAttr(path, list);
        }
        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            return base.MkDir(path, mode);
        }
        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.Open(path, ref fi);
        }
        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.OpenDir(path, ref fi);
        }
        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            return base.Read(path, offset, buffer, ref fi);
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            return base.ReadDir(path, offset, flags, content, ref fi);
          
        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer)
        {
            return base.ReadLink(path, buffer);
        }
        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            base.Release(path, ref fi);
        }
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.ReleaseDir(path, ref fi);
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            return base.RemoveXAttr(path, name);
        }
        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            return base.Rename(path, newPath, flags);
        }
        public override int RmDir(ReadOnlySpan<byte> path)
        {
            return base.RmDir(path);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            return base.SetXAttr(path, name, data, flags);
        }
        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            return base.StatFS(path, ref statfs);
        }
        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target)
        {
            return base.SymLink(path, target);
        }
        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            return base.Truncate(path, length, fiRef);
        }
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            return base.Unlink(path);
        }
        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            return base.UpdateTimestamps(path, ref atime, ref mtime, fiRef);
        }
        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi)
        {
            return base.Write(path, off, span, ref fi);
        }
    }
}
