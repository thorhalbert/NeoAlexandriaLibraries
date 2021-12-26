using NeoAssets.Mongo;
using NeoCommon;
using Tmds.Linux;

namespace NeoVirtFS
{
    public unsafe abstract class FileDescriptor  // All the derived classes should be INeoVirtFile
    {
        private ulong fd1 = ulong.MaxValue;
        public ulong fd {
            get => fd1;
            set {
                if (fd1 != ulong.MaxValue)
                    throw new ArgumentException("Can't Change FD on FileDescriptor", "fd");
                fd1 = value;
            }
        }
        public NeoAssets.Mongo.NeoVirtFS FileNode { get; set; }
        public INeoVirtFile Handler { get; set; }


        public FileDescriptor(NeoAssets.Mongo.NeoVirtFS myFile)
        {
            this.FileNode = myFile;
            this.Handler = (INeoVirtFile) this;
        }

        public static FileDescriptor FileHandlerFactory(NeoAssets.Mongo.NeoVirtFS myFile)
        {
            switch (myFile.Content.ContentType)
            {
                case VirtFSContentTypes.NotAFile:
                    return new FileDescriptorNotFile(myFile);
                case VirtFSContentTypes.Asset:
                    return new FileDescriptorAsset(myFile);
                case VirtFSContentTypes.MountedVolume:
                    return new FileDescriptorMounted(myFile);
                case VirtFSContentTypes.PhysicalFile:
                    return new FileDescriptorPhysical(myFile);
                case VirtFSContentTypes.CachePool:
                    return new FileDescriptorCache(myFile);

                default:
                    throw new ArgumentException($"Unknown content type {myFile.Content.ContentType}", "myFile");
            }

        }

        public int Open(ReadOnlySpan<byte> path, int flags)
        {
            var iot = LibC.open(toBp(path), flags);
            if (iot < 0)
                return -LibC.errno;

            return iot;
        }

        public int Create(ReadOnlySpan<byte> path, mode_t mode, int flags)
        {
            var iot = LibC.open(toBp(path), flags, mode);
            if (iot < 0)
                return -LibC.errno;

            // Make these settable (these actually come in on the Init method, which TDMS isn't handling yet)
            var uid = (uid_t) 10010;
            var gid = (gid_t) 10010;

            var res = LibC.chown(toBp(path), uid, gid);

            return iot;
        }
        public int Write(int chan, ulong off, ReadOnlySpan<byte> buffer)
        {
            ssize_t res;
            fixed (void* vbuf = buffer)
            {
                res = LibC.pwrite((int) chan, vbuf, buffer.Length, (long) off);
            }

            if (res < 0)
                return -LibC.errno;

            return (int) res;
        }
        public int Read(int chan, ulong offset, Span<byte> buffer)
        {
           
            ssize_t res;
            fixed (void* vbuf = buffer)
            {
                res = LibC.pread((int) chan, vbuf, buffer.Length, (long) offset);
            }

            if (res < 0)
                return -LibC.errno;

            return (int) res;

        }
        public void Release(int chan)
        {
            LibC.close(chan);
        }

        public unsafe byte* toBp(ReadOnlySpan<byte> path)
        {
            return RawDirs.ToBytePtr(path.ToArray());
        }
    }

   

  

 

  


}
