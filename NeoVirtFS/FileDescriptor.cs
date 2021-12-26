using NeoAssets.Mongo;

namespace NeoVirtFS
{
    public abstract class FileDescriptor
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
        public NeoVirtFSContent Content { get; set; }
        public INeoVirtFile Handler { get; set; }
        public bool Create;

        public FileDescriptor(NeoVirtFSContent Content, INeoVirtFile Handler, bool Create)
        {
            this.Content = Content;
            this.Handler = Handler;
            this.Create = Create;
        }

        public FileDescriptor FileHandlerFactory(NeoAssets.Mongo.NeoVirtFS myFile, bool create)
        {
            switch (myFile.Content.ContentType)
            {
                case VirtFSContentTypes.NotAFile:
                    return new FileDescriptorNotFile(myFile.Content, new VirtFileNotFile(), create);
                case VirtFSContentTypes.Asset:
                    return new FileDescriptorAsset(myFile.Content, new VirtFileAsset(myFile), create);
                case VirtFSContentTypes.MountedVolume:
                    return new FileDescriptorMounted(myFile.Content, new VirtFileMounted(myFile), create);
                case VirtFSContentTypes.PhysicalFile:
                    return new FileDescriptorPhysical(myFile.Content, new VirtFilePhysical(myFile), create);
                case VirtFSContentTypes.CachePool:
                    return new FileDescriptorCache(myFile.Content, new VirtFileCache(myFile), create);

                default:
                    throw new ArgumentException($"Unknown content type {myFile.Content.ContentType}", "myFile");
            }

        }
    }

   

  

 

  


}
