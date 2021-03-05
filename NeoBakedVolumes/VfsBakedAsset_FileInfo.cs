using MongoDB.Driver;
using NeoCommon.VfsInterfaces;
using System;
using System.Runtime.CompilerServices;
using Tmds.Linux;

namespace NeoBakedVolumes
{
    public class VfsBakedAsset_FileInfo : IVfsFileInfo
    {
        private IMongoDatabase db;
        private VfsBakedAssets vfsBakedAssets;
        private IVfsPath path;
        private IVfsCommon com;

        public VfsBakedAsset_FileInfo(IMongoDatabase db, IMongoCollection<Mongo.BakedAssets> bac, VfsBakedAssets vfsBakedAssets, IVfsPath path)
        {
            this.db = db;
            this.vfsBakedAssets = vfsBakedAssets;
          
            this.path = path; 
        }

        IVfsPath IVfsFileInfo.Filename { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
     
        internal void _attachCom(VfsBakedAsset_Common com)
        {
            throw new NotImplementedException();
        }

        public stat stat { get => com.stat; set => com.stat = value; }
        public bool FileSystemReadOnly { get => com.FileSystemReadOnly; set => com.FileSystemReadOnly = value; }
        IVfsPath IVfsCommon.Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int fd { get; private set; }

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

        int IVfsFileInfo.Open()
        {
            var f = Wrap_LibC.Open(path, 0);
            if (f < 0)
                return f;

            fd = f;
            return f;
        }

        void IVfsFileInfo.Release()
        {
            if (fd != -1)
                LibC.close(fd);
            fd = -1;
        }

        int IVfsFileInfo.Read(ulong offset, Span<byte> buffer)
        {
            return Wrap_LibC.Read(fd, offset, buffer);
        }

        int IVfsFileInfo.Create(mode_t mode)
        {
            return -LibC.EACCES;
        }

        int IVfsFileInfo.Truncate(ulong length)
        {
            return -LibC.EACCES;

        }

        int IVfsFileInfo.Write(ulong offset, ReadOnlySpan<byte> buffer)
        {
            return -LibC.EACCES;

        }
    }
}