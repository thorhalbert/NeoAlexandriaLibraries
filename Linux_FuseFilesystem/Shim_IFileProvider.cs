using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Fuse;

namespace Linux_FuseFilesystem
{
    public class Shim_IFileProvider : FuseFileSystemBase
    {
        // Abstract an IFileProvider to be our filesystem shared out with fuse
        // It may turn out that IFileProvider is inadequate and I may need to 
        // Add an upper level interface that microsoft doesn't (filesystem things--
        // like get free space and such)

        // Is is the intention to be able to 'mount' IFileProviders into 3 separate 
        // mountpoints:
        //  /NARP - the narp filesystem -- overlay on disk - this one might be writable (for mirrors)
        //  /Curated - the curated filesystem - complete database and abstract - may touch real files (if not baked)
        //  /NEOASSET - direct access to the baked files (and the symbolic link placed on real annealed files

        // Have to figure out a CompositeFileProvider which allows to you specify a vfs 'mountpoint'
        // Might have to write our own -- or place the vfs router in here.

        public IFileProvider Fs { get; private set; }

        public Shim_IFileProvider(IFileProvider inFs)
        {
            Fs = inFs;
        }

        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return base.OpenDir(path, ref fi);
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
          
            return base.ReadDir(path, offset, flags, content, ref fi);
        }
    }
}
