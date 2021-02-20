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

        public VfsBakedAsset_Common(IMongoDatabase db, IMongoCollection<BakedAssets> bac, VfsBakedAssets vfsBakedAssets, VfsBakedAsset_Directory dir, VfsBakedAsset_FileInfo file, IVfsPath path)
        {
            this.db = db;
            this.bac = bac;
            this.vfsBakedAssets = vfsBakedAssets;
            this.dir = dir;
            this.file = file;
            this.path = path;
        }

        IVfsPath IVfsCommon.Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        stat IVfsCommon.stat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IVfsCommon.FileSystemReadOnly { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IVfsCommon.Access(IVfsPath path, mode_t mode)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.ChMod(IVfsPath path, mode_t mode, IVfsCommon fiRef)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Chown(IVfsPath path, uint uid, uint gid, IVfsCommon fiRef)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.FAllocate(IVfsPath path, int mode, ulong offset, long length, ref IVfsCommon fi)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Flush(IVfsPath path, ref IVfsCommon fi)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.FSync(IVfsPath path, ref IVfsCommon fi)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.GetAttr(IVfsPath path, ref stat stat, IVfsCommon fiRef)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.GetXAttr(IVfsPath path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Link(IVfsPath fromPath, IVfsPath toPath)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.ListXAttr(IVfsPath path, Span<byte> list)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.ReadLink(IVfsPath path, Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.RemoveXAttr(IVfsPath path, ReadOnlySpan<byte> name)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Rename(IVfsPath path, IVfsPath newPath, int flags)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.SetXAttr(IVfsPath path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.SymLink(IVfsPath path, IVfsPath target)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.Unlink(IVfsPath path)
        {
            throw new NotImplementedException();
        }

        int IVfsCommon.UpdateTimestamps(IVfsPath path, ref timespec atime, ref timespec mtime, IVfsCommon fiRef)
        {
            throw new NotImplementedException();
        }
    }
}