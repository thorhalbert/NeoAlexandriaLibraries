using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using NeoCommon.VfsInterfaces;
using System;
using Tmds.Linux;

namespace NeoBakedVolumes
{
    internal class VfsBakedAsset_Common : IVfsCommon
    {
        private IMongoDatabase db;
        private IMongoCollection<BakedAssets> bac;
        private VfsBakedAssets vfsBakedAssets;
        private VfsBakedAsset_Directory dir;
        private VfsBakedAsset_FileInfo file;
        private IVfsPath path;

        IVfsCommon com;

        public VfsBakedAsset_Common(IMongoDatabase db, IMongoCollection<BakedAssets> bac, VfsBakedAssets vfsBakedAssets, VfsBakedAsset_Directory dir, VfsBakedAsset_FileInfo file, IVfsPath path)
        {
            this.db = db;
            this.bac = bac;
            this.vfsBakedAssets = vfsBakedAssets;
            this.dir = dir;
            this.file = file;
            this.path = path;

            if (dir!=null)
                com = dir;
            if (file != null)
                com = file;
        }

        public IVfsPath Path { get => com.Path; set => com.Path = value; }
        public stat stat { get => com.stat; set => com.stat = value; }
        public bool FileSystemReadOnly { get => com.FileSystemReadOnly; set => com.FileSystemReadOnly = value; }

        public int Access(mode_t mode)
        {
            return com.Access(mode);
        }

        public int Chown(uint uid, uint gid)
        {
            return com.Chown(uid, gid);
        }

        public int FAllocate(int mode, ulong offset, long length)
        {
            return com.FAllocate(mode, offset, length);
        }

        public int Flush(ref IVfsCommon fi)
        {
            return com.Flush(ref fi);
        }

        public int FSync(ref IVfsCommon fi)
        {
            return com.FSync(ref fi);
        }

        public int GetAttr(ref stat stat)
        {
            return com.GetAttr(ref stat);
        }

        public int GetXAttr(ReadOnlySpan<byte> name, Span<byte> data)
        {
            return com.GetXAttr(name, data);
        }

        public int Link(IVfsPath toPath)
        {
            return com.Link(toPath);
        }

        public int ListXAttr(Span<byte> list)
        {
            return com.ListXAttr(list);
        }

        public int ReadLink(Span<byte> buffer)
        {
            return com.ReadLink(buffer);
        }

        public int RemoveXAttr(ReadOnlySpan<byte> name)
        {
            return com.RemoveXAttr(name);
        }

        public int Rename(IVfsPath newPath, int flags)
        {
            return com.Rename(newPath, flags);
        }

        public int SymLink(IVfsPath target)
        {
            return com.SymLink(target);
        }

        public int Unlink()
        {
            return com.Unlink();
        }

        public int UpdateTimestamps(ref timespec atime, ref timespec mtime)
        {
            return com.UpdateTimestamps(ref atime, ref mtime);
        }
    }
}