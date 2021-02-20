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

    

        public VfsBakedAsset_Directory(IMongoDatabase db, IMongoCollection<Mongo.BakedAssets> bac, VfsBakedAssets vfsBakedAssets, IVfsPath path)
        {
            this.db = db;
            this.vfsBakedAssets = vfsBakedAssets;
            this.path = path;

          
        }

        IVfsPath IVfsCommon.Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal void _attachCom(VfsBakedAsset_Common com)
        {
            throw new NotImplementedException();
        }

        stat IVfsCommon.stat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IVfsCommon.FileSystemReadOnly { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IVfsCommon.Access(mode_t mode)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Chown(uint uid, uint gid)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.FAllocate(int mode, ulong offset, long length)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Flush(ref IVfsCommon fi)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.FSync(ref IVfsCommon fi)
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.FSyncDir(IVfsPath readOnlySpan, bool onlyData)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.GetAttr(ref stat stat)
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

        int IVfsCommon.GetXAttr(ReadOnlySpan<byte> name, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Link(IVfsPath toPath)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.ListXAttr(Span<byte> list)
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.MkDir(mode_t mode)
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.OpenDir()
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.ReadDir(ulong offset)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.ReadLink(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.ReleaseDir()
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.RemoveXAttr(ReadOnlySpan<byte> name)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Rename(IVfsPath newPath, int flags)
        {
            throw new NotImplementedException();
        }

        int IVfsDirectoryInfo.RmDir()
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.SymLink(IVfsPath target)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Unlink()
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.UpdateTimestamps(ref timespec atime, ref timespec mtime)
        {
            throw new NotImplementedException();
        }
    }
}