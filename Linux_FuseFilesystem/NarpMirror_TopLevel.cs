using NeoCommon;
using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Fuse;
using Tmds.Linux;

namespace Linux_FuseFilesystem
{
    public class NarpMirror_TopLevel : FuseMountableBase
    {
        private bool debug = false;
        public Dictionary<byte[], MountPoint> Mountpoints { get;  set; }
      

        public NarpMirror_TopLevel(Dictionary<byte[], MountPoint> mountpoints=null) {
            // This ultimately has to be dynamic if these are refreshed
            Mountpoints = mountpoints;

            
        }

        // We're just handling the root (/)
        // All the fileopen stuff should be up the mount level

        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            if (debug) Console.WriteLine($"TopLevel::ReadDir({RawDirs.HR(path)})");
            return 0;
        }
        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi, Guid fileGuid)
        {
            if (debug) Console.WriteLine($"TopLevel::ReadDir({RawDirs.HR(path)},{offset},{flags})");

            try
            {
                content.AddEntry(".");
                content.AddEntry("..");

                foreach (var v in Mountpoints)
                    content.AddEntry(v.Key.AsSpan());

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TopLevel::ReadDir() - error {ex.Message}");
                return -LibC.EACCES;
            }
        }
        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi, Guid fileGuid)
        {
            return 0;
        }
        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data, Guid fileGuid)
        {
            return -LibC.ENOSYS;
        }
        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer, Guid fileGuid)
        {
            return 0;
        }

        // Not sure this will get hit.   We may need to return / itself
        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef, Guid fileGuid)
        {
            if (debug) Console.WriteLine($"TopLevel::GetAttr({RawDirs.HR(path)})");

            if (path.SequenceEqual(Encoding.ASCII.GetBytes("/").AsSpan())) {
                stat.st_nlink = 1;
                stat.st_mode = LibC.S_IFDIR | (mode_t) 0b111_110_110;   // 444 protection for now
                stat.st_uid = 10010; // aRec.Stat.uid;
                stat.st_gid = 10010; // aRec.Stat.gid;

                stat.st_size = 0;

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                stat.st_ctim = new timespec
                {
                    tv_sec = now,
                    tv_nsec = 0
                };
                stat.st_mtim = new timespec
                {
                    tv_sec = now,
                    tv_nsec = 0
                };
                stat.st_atim = new timespec
                {
                    tv_sec = now,
                    tv_nsec = 0
                };

                return 0;
            }

            return -LibC.ENOSYS;
        }
    }
}
