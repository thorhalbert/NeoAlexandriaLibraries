using MongoDB.Bson;
using NeoAssets.Mongo;
using Tmds.Linux;

namespace NeoVirtFS
{
    /// <summary>
    /// This is sort of the filesystems version of a symbolic link.
    /// You will be able to attach another volume at any point within its 
    /// file heirarchy at a point within another filesystem.
    /// 
    /// The only links are the volumeid and the fileid within the volume.
    /// Doing the path mapping will have to happen at upper levels and will be a pain.
    /// 
    /// At this point we're implementing enough so we can do something with the 'archive links' that
    /// PurgeContainer made.  
    /// </summary>
    internal class FileDescriptorMounted : FileDescriptor, INeoVirtFile
    {
        readonly ObjectId VolumeId;
        readonly ObjectId MountPoint;

        public FileDescriptorMounted(NeoAssets.Mongo.NeoVirtFS myFile) : base(myFile)
        {
            if (!myFile.Content.MountedVolume.HasValue ||
                !myFile.Content.AtFilePath.HasValue)
                throw new ArgumentException("MountedVolume or AtFilePath not set");

            VolumeId = myFile.Content.MountedVolume.Value;
            MountPoint = myFile.Content.AtFilePath.Value;
        }

        public int Create(FileDescriptor fds, mode_t mode, int flags)
        {
            return -LibC.EROFS;  // Read only file system
        }

        public int Open(FileDescriptor fds, int flags)
        {
            throw new NotImplementedException();
        }

        public int Read(FileDescriptor fds, ulong offset, Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public int Release(FileDescriptor fds)
        {
            return 0;
        }

        public int Write(FileDescriptor fds, ulong off, ReadOnlySpan<byte> span)
        {
            return -LibC.EROFS;  // Read only file system
        }
    }

}
