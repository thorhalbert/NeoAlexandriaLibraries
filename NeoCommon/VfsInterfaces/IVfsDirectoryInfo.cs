using System;
using System.Collections;
using System.Collections.Generic;
using Tmds.Linux;

namespace NeoCommon.VfsInterfaces
{
    public interface IVfsDirectoryInfo : IEnumerable<IVfsFileInfo>, IEnumerable, IVfsCommon
    {

        int ReadDir(IVfsPath path, ulong offset, ref IVfsDirectoryInfo fi);
        int OpenDir(IVfsPath path, ref IVfsDirectoryInfo fi);
        int ReleaseDir(IVfsPath path, ref IVfsDirectoryInfo fi);
        int FSyncDir(IVfsPath readOnlySpan, bool onlyData, ref IVfsDirectoryInfo fi);

        int RmDir(IVfsPath path);
        int MkDir(IVfsPath path, mode_t mode);

    }
}