using MongoDB.Driver;
using NeoCommon.VfsInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using Tmds.Linux;

namespace NeoBakedVolumes
{
    internal class VfsBakedAsset_Directory : IVfsDirectoryInfo
    {
        private IMongoDatabase db;
        private VfsBakedAssets vfsBakedAssets;
        private IVfsPath path;

        IVfsCommon com;
    

        public VfsBakedAsset_Directory(IMongoDatabase db, IMongoCollection<Mongo.BakedAssets> bac, VfsBakedAssets vfsBakedAssets, IVfsPath path)
        {
            this.db = db;
            this.vfsBakedAssets = vfsBakedAssets;
            this.path = path;

          
        }

        IVfsPath IVfsCommon.Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal void _attachCom(VfsBakedAsset_Common com)
        {
            this.com = com;
        }

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

        int IVfsDirectoryInfo.ReadDir(ulong offset)
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.OpenDir()
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.ReleaseDir()
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.FSyncDir(IVfsPath readOnlySpan, bool onlyData)
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.RmDir()
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.MkDir(mode_t mode)
        {
            throw new NotImplementedException();
        }

        IEnumerator<IVfsFileInfo> IEnumerable<IVfsFileInfo>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}