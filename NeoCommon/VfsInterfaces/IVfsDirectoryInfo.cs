using System;
using System.Collections;
using System.Collections.Generic;
using Tmds.Linux;

namespace NeoCommon.VfsInterfaces
{
    public interface IVfsDirectoryInfo : IEnumerable<IVfsFileInfo>, IEnumerable, IVfsCommon
    {
        int ReadDir(ulong offset);
        int OpenDir();
        int ReleaseDir();
        int FSyncDir(IVfsPath readOnlySpan, bool onlyData);

        int RmDir();
        int MkDir(mode_t mode);
    }
}