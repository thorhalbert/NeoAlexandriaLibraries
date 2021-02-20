using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Linux;

namespace NeoCommon.VfsInterfaces
{
    /// <summary>
    /// Though it's quite cool, the IFileProvider may not be very good for virtual filesystems.
    /// Real filenames are byte arrays, not unicode strings.  I had to deal with this extensively
    /// in python.  You just can't control what you get in a filename and it's quite arrogant to 
    /// assert that it's the filename that's the problem.   It's going to be easy enough to extend
    /// this to provide strings, rather than having to work around the fact that you have to 
    /// encode/decode strings (of possibily unknown/unknowable encoding) every time you need them.
    /// 
    /// Also, filesystem operations might not be best done with streams, but rather the low level
    /// operations that you might expect, at least in linux (the normal libC calls for I/O).  I'm
    /// hoping that the fuse filesystem for example just needs to wrap this interface.   Not sure 
    /// about Dokan (and possibly webdav -- if I can get those to run).
    /// 
    /// I also required an abstraction that the CompositeFileProvider didn't have (at least yet), which
    /// was a 'mountpoint', where the filesystem you inject could then occupy some contrived path in the vfs.
    /// 
    /// We'll also handle a few real vfs operations, like free space that are nice on fuse.
    /// 
    /// So, first a sketch.
    /// </summary>
    public interface IVfsFilesystem
    {
        IVfsDirectoryInfo DirectoryInfo(IVfsPath path);
        IVfsFileInfo FileInfo(IVfsPath path);


        int StatFS(IVfsPath path);
    }
}
