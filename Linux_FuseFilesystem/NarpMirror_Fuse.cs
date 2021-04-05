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

        readonly byte[] fakeFile = Encoding.ASCII.GetBytes("/dev/null");

        public NarpMirror_Fuse()
        {
            base.debug = false;

            if (debug) Console.WriteLine($"NeoFS::NarpMirror_Fuse() constructor");
            //SupportsMultiThreading = false;   
        }

        //public override bool SupportsMultiThreading => false;

        // File read directories
        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {

            if (debug) Console.WriteLine($"NeoFS::OpenDir({RawDirs.HR(path)})");
            //path = base.TransformPath(path);
            return 0;
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi, Guid fileGuid)
        {
            if (debug) Console.WriteLine($"NeoFS::ReadDir({RawDirs.HR(path)},{offset},{flags})");
            path = base.TransformPath(path);

            // We only have the low level calls, so we need to do opendir/readdir ourself

            var dirFd = RawDirs.opendir(RawDirs.ToNullTerm(path.ToArray()));
            if (dirFd == IntPtr.Zero)
                return 0;

            content.AddEntry(".");  // This can eat strings
            content.AddEntry("..");

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
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            if (debug) Console.WriteLine($"NeoFS::ReleaseDir({RawDirs.HR(path)})");
            path = base.TransformPath(path);
            return 0;
        }
        public override int FSyncDir(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::FSyncDir()");

            LibC.sync();

            return 0; // base.FSyncDir(readOnlySpan, onlyData, ref fi);
        }

        // File read

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            path = base.TransformPath(path);
            if (path.Length < 1) return -LibC.ENOENT;

            if (debug) Console.WriteLine($"NeoFS::Open()");

            // Stat the file

            var stat = new stat();

            var lc = LibC.lstat(toBp(path), &stat);
            if (lc < 0)
                return -LibC.errno;

            string extAssetSha1 = null;

            // Intercept an asset - we must provide the asset's Attr, not the link of the baked file
            if ((stat.st_mode & LibC.S_IFMT) == LibC.S_IFLNK)
            {
                if (debug) Console.WriteLine($"Stat mode={stat.st_mode}, S_IFMT={LibC.S_IFMT}, link={LibC.S_IFLNK}");

                // Get the link

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
                    link = MemoryExtensions.AsSpan<byte>(buffer, 0, (int) retl);
                    if (debug) Console.WriteLine($"Found ASSET {RawDirs.HR(link)}");

                    var asset = link.Slice(assetTag.Length);
                    extAssetSha1 = Encoding.ASCII.GetString(asset);
                }
                else
                {
                    link = MemoryExtensions.AsSpan<byte>(buffer, 0, (int) retl);

                    //path = base.TransformPath()   - have to figure out how to pass to our mountmapper
                    // Probably a dependancy injection
                    // These links shouldn't happen, but it would be a nice attack vector
                }
            }

            // See if asset -- if so, set to asset mode and call base

            if (extAssetSha1 != null)  // Asset Mode
            {
                var fakeFd = LibC.open(toBp(fakeFile), 0);
                if (fakeFd > 0)
                {
                    fi.fh = (ulong) fakeFd;
                    FileContexts.Add(fi.fh, new FileContext
                    {
                        AssetLink = null,
                        ExtAssetSha1 = extAssetSha1,
                        ExtFileHandle = null,
                    });
                    //Console.WriteLine($"Create Context (sha1) ID={fi.fh}");
                }
                else
                    Console.WriteLine($"Error Opening /dev/null?  Error={LibC.errno}");
                return base.AssetOpen(path, ref fi, fileGuid);
            }

            // transform the link

            var newFd = LibC.open(toBp(path), fi.flags);
            if (newFd > 0)
            {
                fi.fh = (ulong) newFd;

                // Make a context anyway
                FileContexts.Add(fi.fh, new FileContext
                {
                    AssetLink = null,
                    ExtAssetSha1 = null,
                    ExtFileHandle = null,
                });
                //Console.WriteLine($"Create Context (null) ID={fi.fh}");

                return 0;
            }

            return -LibC.errno;
        }
        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi, Guid fileGuid)
        {
            var context = FileContexts[fi.fh];

            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Read()");

            if (context.ExtAssetSha1 != null)  // Asset Mode
                return base.AssetRead(path, offset, buffer, ref fi, fileGuid);

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
                res = LibC.pread((int) fi.fh, vbuf, buffer.Length, (long) offset);
            }

            if (res < 0)
                return -LibC.errno;

            return (int) res;

        }
        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) 
                Console.WriteLine($"NeoFS::Release({RawDirs.HR(path)})");

            if (FileContexts.TryGetValue(fi.fh, out var context))
            {
                if (context.ExtAssetSha1 != null)  // Asset Mode
                    base.AssetRelease(path, ref fi, fileGuid);
                FileContexts.Remove(fi.fh);
            }

            if (fi.fh > 0)
                LibC.close((int) fi.fh);

            fi.fh = 0;
        }
        public override int FSync(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::FSync()");
            return 0; // base.FSync(path, ref fi);
        }

        // Metadata read
        public override int Access(ReadOnlySpan<byte> path, mode_t mode, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug)
              Console.WriteLine($"NeoFS::Access({RawDirs.HR(path)},{mode}");

            var res = LibC.access(toBp(path), (int) mode);
            if (res < 0)
                return -LibC.errno;

            return 0; // base.Access(path, mode);
        }
        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef, Guid fileGuid)
        {
            try
            {
                path = base.TransformPath(path);

                if (debug)
                    Console.WriteLine($"NeoFS::GetAttr({RawDirs.HR(path)})");
                fixed (stat* st = &stat)
                {
                    var lc = LibC.lstat(toBp(path), st);
                    if (lc < 0)
                    {
                        // Windows file explorer hits lot of files that don't exist
                        //Console.WriteLine($"Stat error: {lc} errno={LibC.errno} path={RawDirs.HR(path)}");
                        return -LibC.errno;
                    }
                }

                // Intercept an asset - we must provide the asset's Attr, not the link of the baked file
                if ((stat.st_mode & LibC.S_IFMT) == LibC.S_IFLNK)
                {
                    if (debug) Console.WriteLine($"Link Seen: Stat mode={stat.st_mode}, S_IFMT={LibC.S_IFMT}, link={LibC.S_IFLNK}");
                    ssize_t retl;
                    var buffer = new byte[1024];
                    fixed (byte* b = buffer)
                    {
                        retl = LibC.readlink(toBp(path), b, buffer.Length);
                    }

                    // Trying to do something like startswith for bytes that's fast
                    var link = MemoryExtensions.AsSpan(buffer, 0, assetTag.Length);

                    if (debug) Console.WriteLine($"NeoFS::GetAttr Asset Detected - ({RawDirs.HR(path)}, {RawDirs.HR(link)}");

                    if (link.SequenceEqual(assetTag))
                    {
                        link = MemoryExtensions.AsSpan(buffer, 0, (int) retl);
                        if (debug) Console.WriteLine($"Found ASSET {RawDirs.HR(link)}");

                        base.GetAssetAttr(path, link, ref stat, fileGuid);
                        return 0;
                    }
                }

                if (debug) Console.WriteLine($"Stat Dump: size={stat.st_size}, mode={stat.st_mode}, mtim={stat.st_mtim}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAttr error: {ex.Message} {ex.StackTrace}");
                return -LibC.ENOENT;
            }
        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer, Guid fileGuid)
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
        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Create()");

            fi.fh = 0;

            var res = LibC.open(toBp(path), fi.flags, mode);
            if (res < 0)
                return -LibC.errno;

            fi.fh = (ulong) res;

            // Make these settable (these actually come in on the Init method, which TDMS isn't handling yet)
            var uid = (uid_t) 10010;
            var gid = (gid_t) 10010;

            res = LibC.chown(toBp(path), uid, gid);
            
            return 0;
        }
        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> buffer, ref FuseFileInfo fi, Guid fileGuid)
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
        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            path = base.TransformPath(path);



            if (debug) Console.WriteLine($"NeoFS::Flush()");



            return 0; // base.Flush(path, ref fi);
        }
        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi, Guid fileGuid)
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


            var res = LibC.fallocate((int) fi.fh, mode, (long) offset, length);


            if (res < 0)
                return -LibC.errno;

            return (int) res;
        }
        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef, Guid fileGuid)
        {
            path = base.TransformPath(path);
            if (debug) Console.WriteLine($"NeoFS::Truncate()");

            var res = LibC.truncate(toBp(path), (long) length);

            if (res < 0) res = -LibC.errno;

            return res;
        }
        // Set metadata
        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::ChMod()");

            var res = LibC.chmod(toBp(path), mode);
            if (res < 0) return -LibC.errno;
            return 0;
        }
        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Chown()");
            var res = LibC.chown(toBp(path), uid, gid);
            if (res < 0) return -LibC.errno;
            return 0;
        }
        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath, Guid fileGuid)
        {
            if (debug) Console.WriteLine($"NeoFS::Link()");
            fromPath = base.TransformPath(fromPath);
            toPath = base.TransformPath(toPath);

            var res = LibC.link(toBp(fromPath), toBp(toPath));
            if (res < 0)
                res = LibC.errno;

            return res;
        }

        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target, Guid fileGuid)
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
        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::UpdateTimestamps()");

            timespec[] timeList = new timespec[2];
            timeList[0].tv_sec = atime.tv_sec;
            timeList[0].tv_nsec = atime.tv_nsec;
            timeList[1].tv_sec = mtime.tv_sec;
            timeList[1].tv_nsec = mtime.tv_nsec;

            fixed (timespec* timeA = &timeList[0])
            { 
                var res = LibC.utimensat(0, toBp(path), timeA, 0);
                if (res < 0)
                    return -LibC.errno;
            }

            return 0;
        }

        // Write directory metadata
        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::MkDir()");

            var res = LibC.mkdir(toBp(path), mode);
            if (res < 0)
                return -LibC.errno;

            return 0;
        }
        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags, Guid fileGuid)
        {
            path = base.TransformPath(path);
            newPath = base.TransformPath(newPath);

           // if (debug) 
                Console.WriteLine($"NeoFS::Rename({RawDirs.HR(path)}, {RawDirs.HR(newPath)})");

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
        public override int RmDir(ReadOnlySpan<byte> path, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::RmDir()");
            var res = LibC.rmdir(toBp(path));
            if (res < 0)
                return -LibC.errno;

            return 0;
        }
        public override int Unlink(ReadOnlySpan<byte> path, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::Unlink()");
            var res = LibC.unlink(toBp(path));
            if (res < 0)
                return -LibC.errno;

            return 0;
        }

        // Filesystem level 
        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs, Guid fileGuid)
        {
            path = base.TransformPath(path);

            if (debug) Console.WriteLine($"NeoFS::StatFS()");

            int res;
            fixed (statvfs* vfs = &statfs)
            {
                res = LibC.statvfs(toBp(path), vfs);
            }

            if (res < 0) return -LibC.errno;

            return 0;
        }
        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data, Guid fileGuid)
        {
            return -LibC.ENOSYS;
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
