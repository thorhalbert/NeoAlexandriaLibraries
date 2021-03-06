﻿using Logos.Utility;
using NeoCommon;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Fuse;
using Tmds.Linux;
using VDS.Common.Tries;

namespace Linux_FuseFilesystem
{
    public class MountPoint
    {
        public byte[] Name;
        public byte[] MountFrom;
        public byte[] MountTo;
        public FuseMountableBase fuse;

        public MountPoint(ReadOnlySpan<byte> name, ReadOnlySpan<byte> mountFrom, ReadOnlySpan<byte> mountTo, FuseMountableBase fmb)
        {
            Name = name.ToArray();
            MountFrom = mountFrom.ToArray();
            MountTo = mountTo.ToArray();
            fuse = fmb;

            cleanPoint(MountFrom);
            cleanPoint(MountTo);
        }
      

        internal void Remount(ReadOnlySpan<byte> mountFrom, ReadOnlySpan<byte> mountTo, FuseMountableBase fsys)
        {
            MountFrom = mountFrom.ToArray();
            MountTo = mountTo.ToArray();
            fuse = fsys;   // We may need to dispose of fmb more elegantly

            cleanPoint(MountFrom);
            cleanPoint(MountTo);
        }

        internal void cleanPoint(byte[] inMount)
        {
            return;
        }
    }
    /// <summary>
    /// Overlay/Union Dispatch Filesystem Layer 
    //  Various filesystems are 'mounted' here and then based on the path they get dispatched to whichever 
    //  nested layer that has been mounted underneath
    /// </summary>  
    public class NarpMirror_MountDispatch : FuseFileSystemBase
    {
        // dispatch 
        Dictionary<byte[], MountPoint> mapSys = new Dictionary<byte[], MountPoint>();
        readonly Trie<byte[], byte, MountPoint> mountTrie = new Trie<byte[], byte, MountPoint>(KeyMapper);
        FuseMountableBase defaultLayer;
        bool debug = false;

        public Dictionary<byte[], MountPoint> MapSys { get => mapSys; set => mapSys = value; }


        Dictionary<ulong, FileContext> FileContexts = new Dictionary<ulong, FileContext>();

        public override bool SupportsMultiThreading => false;  // Let's see if this is true

        public NarpMirror_MountDispatch(FuseMountableBase defLayer)
        {
            defaultLayer = defLayer;
        }

        // It is assumed that these mountpoints are already normalized
        /// <summary>
        /// Attach/Mount a lower level filesystem - this acts as a mapping layer
        /// </summary>
        /// <param name="mountFrom"></param>
        /// The mount level inside the original filesystem, if you put like 'bob/fred'
        /// And mount to FRED, then a request to FRED/alice.txt would map to bob/fred/alice.txt
        /// 
        /// So, the mount from is removed, and the mountTo is added (or the reverse on a lookup)
        /// 
        /// No leading /, and this can be empty for a flat mount
        /// <param name="mountTo"></param>
        /// The file system level that the end user sees (or at least the level above this).
        /// 
        /// The actual mountpoint I think is hidden from this so, if we're /mnt/test/FRED/alice.txt
        /// Then with the above scenario I think we still access bob/fred/alice.txt
        /// 
        /// No leading /, and this can also be empty for fully flat (I can see this usecase where one
        /// fs is flat an onother is overlayed on it)
        /// <param name="fsys">
        /// The lower level FuseFileSystemBase that this mount point will dispatch to
        /// </param>
        public void Mount(ReadOnlySpan<byte> name, ReadOnlySpan<byte> mountFrom, ReadOnlySpan<byte> mountTo, FuseMountableBase fsys)
        {
            var n = name.ToArray();
                 
            // See if the mountpoint changed - maybe just need to update mountTo
            if (MapSys.ContainsKey(n))
            {
                var map = MapSys[n];

                mountTrie.Remove(map.MountFrom);
                map.Remount(mountFrom, mountTo, fsys);
                mountTrie.Add(mountFrom.ToArray(), map);
                return;
            }

            var mountPoint = new MountPoint(name, mountFrom, mountTo, fsys);
            MapSys.Add(mountPoint.Name, mountPoint);

            mountTrie.Add(mountFrom.ToArray(), mountPoint);

            fsys.FileContexts = FileContexts;   // Get our context
        }

        public static IEnumerable<byte> KeyMapper(byte[] key)
        {
            return key;
        }

        public ReadOnlySpan<byte> DispatchOn(ReadOnlySpan<byte> path, out FuseMountableBase fsys, ref FuseFileInfo fi, out Guid fileId)
        {
            fsys = null;

            if (debug)
                Console.WriteLine($"InPath: {RawDirs.HR(path)}");


            // Surely we can do something better with these spans

            var byteA = path.ToArray();

            var inPath = new List<byte>(byteA);

            var realPath = new List<byte>(Encoding.ASCII.GetBytes("/NARP"));
            realPath.AddRange(byteA);

            var fileuuid = GuidUtility.Create(GuidUtility.UrlNamespace, realPath.ToArray());
            fileId = fileuuid;

            // normalize(inPath);

            // This did what I expected in testing - it matched on the expected path stub
            var prefixMatch = mountTrie.FindPredecessor(inPath.ToArray());
            if (prefixMatch == null)
            {
                if (debug)
                    Console.WriteLine($"NoMatch: {RawDirs.HR(path)}");
                fsys = defaultLayer;  // Ultimately we may need to handle the top level directories
                return path;  // new byte[0].AsSpan();  // Generates a ENOENT
            }

            var p = prefixMatch.Value;

            fsys = p.fuse;

            inPath.RemoveRange(0, p.MountFrom.Length);
            inPath.InsertRange(0, p.MountTo);

            // Normalize the path (remove ..) - expensive
            // Find quick way to match the lhs of our path to the mountlist - something better than brute force
            //    -- remove the lhs, prepend the rhs

            if (debug) Console.WriteLine($"Map: {RawDirs.HR(p.Name)} - {RawDirs.HR(path)} => {RawDirs.HR(inPath.ToArray())} ({fileuuid})");

            return inPath.ToArray();
        }

        public void normalize(List<byte> inPath)
        {
            var ps = new Stack<byte[]>();

            var lwm = 1;
            while (true)
            {
                var idx = inPath.IndexOf((byte) '/', lwm);
                if (idx < 0) break;

            }
        }

        // File read directories
        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::OpenDir()");

            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.OpenDir(usePath, ref fi, fileGuid);
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::ReadDir()");

            try
            {
                var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
                if (usePath.Length < 1) return -LibC.ENOENT;
                return fs.ReadDir(usePath, offset, flags, content, ref fi, fileGuid);
            }catch(Exception ex)
            {
                Console.WriteLine($"Mount::ReadDir() - error {ex.Message}");
                return -LibC.EACCES;
            }
        }
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::ReleaseDir()");

            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            return fs.ReleaseDir(usePath, ref fi, fileGuid);
        }
        public override int FSyncDir(ReadOnlySpan<byte> readOnlySpan, bool onlyData, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::FSyncDir()");

            var usePath = DispatchOn(readOnlySpan, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.FSyncDir(usePath, onlyData, ref fi, fileGuid);
        }

        // File read

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::Open()");

            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Open(usePath, ref fi, fileGuid);
        }
        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::Read()");

            var usePath = DispatchOn(path, out var fs, ref  fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Read(usePath, offset, buffer, ref fi, fileGuid);
        }
        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::Release()");

            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return;
            fs.Release(usePath, ref fi, fileGuid);
        }
        public override int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::FSync()");

            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.FSync(usePath, ref fi, fileGuid);
        }

        // Metadata read
        public override int Access(ReadOnlySpan<byte> path, mode_t mode)
        {
            if (debug) Console.WriteLine($"Mount::Access()");

            FuseFileInfo fi =new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Access(usePath, mode, fileGuid);
        }
        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            if (debug) Console.WriteLine($"Mount::GetAttr({RawDirs.HR(path)})");


            try
            {
                if (fiRef.IsNull) {
                    FuseFileInfo fi = new FuseFileInfo();
                    fiRef = new FuseFileInfoRef(new Span<FuseFileInfo>(new FuseFileInfo[] { fi }));
                }
              
                var usePath = DispatchOn(path, out var fs, ref fiRef.Value,  out var fileGuid);
                if (usePath.Length < 1) return -LibC.ENOENT;
                return fs.GetAttr(usePath, ref stat, fiRef, fileGuid);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return -LibC.ENOENT;
        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer)
        {
            if (debug) Console.WriteLine($"Mount::ReadLink({RawDirs.HR(path)}");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.ReadLink(usePath, buffer, fileGuid);
        }
        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            if (debug) Console.WriteLine($"Mount::GetXAttr()");
            return -LibC.ENOSYS;

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.GetXAttr(usePath, name, data, fileGuid);
        }
        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            if (debug) Console.WriteLine($"Mount::ListXAttr()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.ListXAttr(usePath, list, fileGuid);
        }
        // Write
        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::Create()");


            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Create(usePath, mode, ref fi,fileGuid );
        }
        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::Write()");

            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Write(usePath, off, span, ref fi, fileGuid);
        }
        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::Flush()");

            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Flush(usePath, ref fi, fileGuid);
        }
        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            if (debug) Console.WriteLine($"Mount::FAllocate()");

            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.FAllocate(usePath, mode, offset, length, ref fi, fileGuid);
        }
        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            if (debug) Console.WriteLine($"Mount::Truncate()");

            var usePath = DispatchOn(path, out var fs, ref fiRef.Value,  out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Truncate(usePath, length, fiRef, fileGuid);
        }
        // Set metadata
        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            if (debug) Console.WriteLine($"Mount::ChMod()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.ChMod(usePath, mode, fiRef, fileGuid);
        }
        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            if (debug) Console.WriteLine($"Mount::Chown()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Chown(usePath, uid, gid, fiRef, fileGuid);
        }
        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
        {
            if (debug) Console.WriteLine($"Mount::Link()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(fromPath, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Link(usePath, toPath, fileGuid);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            if (debug) Console.WriteLine($"Mount::SetXAttr()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs,ref  fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.SetXAttr(usePath, name, data, flags, fileGuid);
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            if (debug) Console.WriteLine($"Mount::RemoveXAttr()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.RemoveXAttr(usePath, name, fileGuid);
        }
        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target)
        {
            if (debug) Console.WriteLine($"Mount::SymLink()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.SymLink(usePath, target, fileGuid);
        }
        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            if (debug) Console.WriteLine($"Mount::UpdateTimestamps()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.UpdateTimestamps(usePath, ref atime, ref mtime, fiRef, fileGuid);
        }

        // Write directory metadata
        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            if (debug) Console.WriteLine($"Mount::MkDir()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.MkDir(usePath, mode, fileGuid);
        }
        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            //if (debug)
                Console.WriteLine($"Mount::Rename({RawDirs.HR(path)},{RawDirs.HR(newPath)})");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs,ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            var toPath = DispatchOn(newPath, out var fs2, ref fi, out var fileGuid2);
            if (usePath.Length < 1) return -LibC.ENOENT;

            // this should fail if these are in different structures
            if (fs != fs2)
                return -LibC.ENOENT;

            return fs.Rename(usePath, toPath, flags, fileGuid);
        }
        public override int RmDir(ReadOnlySpan<byte> path)
        {
            if (debug) Console.WriteLine($"Mount::RmDir()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.RmDir(usePath, fileGuid);
        }
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            if (debug) Console.WriteLine($"Mount::Unlink()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.Unlink(usePath , fileGuid);
        }

        // Filesystem level 
        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            if (debug) Console.WriteLine($"Mount::StatFS()");

            FuseFileInfo fi = new FuseFileInfo();
            var usePath = DispatchOn(path, out var fs, ref fi, out var fileGuid);
            if (usePath.Length < 1) return -LibC.ENOENT;
            return fs.StatFS(usePath, ref statfs, fileGuid);
        }
    }
}
