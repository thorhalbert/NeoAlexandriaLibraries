using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Fuse;
using Tmds.Linux;
using NeoBakedVolumes;
using NeoCommon;

namespace Linux_FuseFilesystem
{
    public unsafe class NarpMirror_Fuse : FuseMountableBase
    {

       
        readonly byte[] assetTag = Encoding.ASCII.GetBytes("/NEOASSET/sha1/");


        public NarpMirror_Fuse()
        {
            base.debug = false;

              if (debug) Console.WriteLine($"NeoFS::NarpMirror_Fuse() constructor");
            //SupportsMultiThreading = true;

        }

        public override bool SupportsMultiThreading => false;

        // File read directories
        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::OpenDir({RawDirs.HR(path)})");
            return 0;
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::ReadDir({RawDirs.HR(path)},{offset},{flags})");

            // We only have the low level calls, so we need to do opendir/readdir ourself

            var dirFd = RawDirs.opendir(RawDirs.ToNullTerm(path.ToArray()));
            if (dirFd == IntPtr.Zero)
                return 0;

            while (true)
            {
                var dir = RawDirs.readdir_wrap(dirFd, false);  // Don't remove the null
                if (dir == null) break;

                var d = dir.Value;

                if (debug) Console.WriteLine($"{RawDirs.HR(path)} -> {RawDirs.HR(d.d_name)}");

                content.AddEntry(d.d_name.AsSpan());
            }

            RawDirs.closedir(dirFd);

            return 0;
        }
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::ReleaseDir({RawDirs.HR(path)})");
            return 0;
        }
        public override int FSyncDir(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::FSyncDir()");

            LibC.sync();

            return 0; // base.FSyncDir(readOnlySpan, onlyData, ref fi);
        }

        // File read

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Open()");

            var newFd = LibC.open(toBp(path), fi.flags);
            if (newFd > 0)
            {
                fi.fh = (ulong) newFd;
                return 0;
            }

            return -LibC.errno;   
        }
        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Read()");
            if (fi.fh == 0)
            {
                var newFd = LibC.open(toBp(path), fi.flags);
                if (newFd > 0)
                    fi.fh = (ulong) newFd;
                else return -LibC.errno;
            }

            ssize_t res;
            fixed (void* vbuf = buffer) {
                res = LibC.pread((int) fi.fh, vbuf, buffer.Length, (long) offset);
            }

            if (res < 0)
                return -LibC.errno;

            return (int) res;

        }
        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Release()");
            if (fi.fh > 0)
                LibC.close((int) fi.fh);

            fi.fh = 0;
        }
        public override int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::FSync()");
            return 0; // base.FSync(path, ref fi);
        }

        // Metadata read
        public override int Access(ReadOnlySpan<byte> path, mode_t mode)
        {
            path = base.TransformPath(path);

            //if (debug)
                Console.WriteLine($"NeoFS::Access({RawDirs.HR(path)},{mode}");

            var res = LibC.access(toBp(path), (int) mode);
            if (res < 0)
                return -LibC.errno;

            return 0; // base.Access(path, mode);
        }
        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            path = base.TransformPath(path);

            if (debug)
                Console.WriteLine($"NeoFS::GetAttr({RawDirs.HR(path)})");
            fixed (stat* st = &stat)
            {
                var lc = LibC.lstat(toBp(path), st);
                if (lc < 0)
                    return -LibC.errno;
            }

            // Intercept an asset - we must provide the asset's Attr, not the link of the baked file
            if ((stat.st_mode & LibC.S_IFMT) == LibC.S_IFLNK)
            {
                if (debug) Console.WriteLine($"Stat mode={stat.st_mode}, S_IFMT={LibC.S_IFMT}, link={LibC.S_IFLNK}");
                ssize_t retl;
                var buffer = new byte[1024];
                fixed (byte* b = buffer)
                {
                    retl = LibC.readlink(toBp(path), b, buffer.Length);
                }

                // Trying to do something like startswith for bytes that's fast
                var link = MemoryExtensions.AsSpan<byte>(buffer, 0, assetTag.Length);

                if (debug) Console.WriteLine($"NeoFS::GetAttr Asset Detected - ({RawDirs.HR(path)}, {RawDirs.HR(link)}");

                if (link.SequenceEqual(assetTag))
                {
                   
                    link = MemoryExtensions.AsSpan<byte>(buffer, 0, (int)retl);
                    if (debug) Console.WriteLine($"Found ASSET {RawDirs.HR(link)}");
                    var g = Guid.Empty;
                    if (!fiRef.IsNull && fiRef.Value.ExtFileHandle.HasValue)
                        g = fiRef.Value.ExtFileHandle.Value;
                    base.GetAssetAttr(path, link, ref stat, g);
                }
            }

            return 0;

        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::ReadLink({RawDirs.HR(path)}");
          
            ssize_t retl;
            fixed (byte* b = buffer)
            {
                retl = LibC.readlink(toBp(path), b, buffer.Length);
            }

            if (retl < 0) return -LibC.errno;

            return 0;
        }
      
        // Write
        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Create()");

            fi.fh = 0;

            var res = LibC.open(toBp(path), fi.flags, mode);
            if (res < 0)
                return -LibC.errno;

            fi.fh = (ulong) res;

            return 0;
        }
        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> buffer, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Write()");
            if (fi.fh == 0)
            {
                var newFd = LibC.open(toBp(path), fi.flags);
                if (newFd > 0)
                    fi.fh = (ulong) newFd;
                else return -LibC.errno;
            }

            ssize_t res;
            fixed (void* vbuf = buffer)
            {
                res = LibC.pwrite((int) fi.fh, vbuf, buffer.Length, (long) off);
            }

            if (res < 0)
                return -LibC.errno;

            return (int) res;
        }
        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

      

            if (debug) Console.WriteLine($"NeoFS::Flush()");



            return 0; // base.Flush(path, ref fi);
        }
        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::FAllocate()");

            if (fi.fh == 0)
            {
                var newFd = LibC.open(toBp(path), fi.flags);
                if (newFd > 0)
                    fi.fh = (ulong) newFd;
                else return -LibC.errno;
            }

  
             var    res = LibC.fallocate((int) fi.fh, mode, (long) offset, length);
            

            if (res < 0)
                return -LibC.errno;

            return (int) res;
        }
        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            path = base.TransformPath(path);
            if (debug) Console.WriteLine($"NeoFS::Truncate()");

            var res = LibC.truncate(toBp(path), (long) length);

            if (res < 0) res = -LibC.errno;

            return res;
        }
        // Set metadata
        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::ChMod()");

            var res = LibC.chmod(toBp(path), mode);
            if (res < 0) return -LibC.errno;
            return 0;
        }
        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Chown()");
            var res= LibC.chown(toBp(path), uid,gid);
            if (res < 0) return -LibC.errno;
            return 0;
        }
        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
        {
            if (debug) Console.WriteLine($"NeoFS::Link()");
            fromPath = base.TransformPath(fromPath);
            toPath = base.TransformPath(toPath);

            var res = LibC.link(toBp(fromPath), toBp(toPath));
            if (res < 0)
                res = LibC.errno;

            return res;
        }
      
        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::SymLink()");
            var fromPath = base.TransformPath(path);
            var toPath = base.TransformPath(target);

            var res = LibC.symlink(toBp(toPath), toBp(fromPath));
            if (res < 0)
                res = LibC.errno;

            return res;
        }
        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::UpdateTimestamps()");

           // LibC.utimensat()

            return base.UpdateTimestamps(path, ref atime, ref mtime, fiRef);
        }

        // Write directory metadata
        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::MkDir()");

            var res = LibC.mkdir(toBp(path), mode);
            if (res < 0)
                return -LibC.errno;

            return 0;
        }
        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            path = base.TransformPath(path);
            newPath = base.TransformPath(newPath);

            if (debug) Console.WriteLine($"NeoFS::Rename()");

            // Without rename call this is actually a bit involved

            // unlink newpath
            // link path newpath
            // unlink path

            var res = LibC.unlink(toBp(newPath));
            // Ignore result - file probably doesn't exist

            res = LibC.link(toBp(path), toBp(newPath));
            if (res < 0)
                return -LibC.errno;

            res = LibC.unlink(toBp(path));
            // We will ignore this too

            return 0;
        }
        public override int RmDir(ReadOnlySpan<byte> path)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::RmDir()");
            var res = LibC.rmdir(toBp(path));
            if (res < 0)
                return -LibC.errno;

            return 0;
        }
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Unlink()");
            var res = LibC.unlink(toBp(path));
            if (res < 0)
                return -LibC.errno;

            return 0;
        }

        // Filesystem level 
        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::StatFS()");

            int res;
            fixed(statvfs* vfs = &statfs)
            {
                res = LibC.statvfs(toBp(path), vfs);
            }

            if (res < 0) return -LibC.errno;

            return 0;
        }

        // Utillity functions 

        public unsafe byte* toNullTerm(ReadOnlySpan<byte> path)
        {
            var l = path.Length;
            var newP = new byte[l + 1];
            Array.Copy(path.ToArray(), newP, l);
            newP[l] = 0;  // Put the terminator on it

            fixed (byte* p = newP)
                return p;
        }

        public unsafe byte* toBp(ReadOnlySpan<byte> path)
        {
            return RawDirs.ToBytePtr(path.ToArray());
        }
    }
}
