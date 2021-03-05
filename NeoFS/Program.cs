using Linux_FuseFilesystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tmds.Fuse;

namespace NeoFS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //string type = args.Length > 0 ? args[0] : "hello";

            if (!Fuse.CheckDependencies())
            {
                Console.WriteLine(Fuse.InstallationInstructions);
                return;
            }

            IFuseFileSystem fileSystem;
            fileSystem = new NarpMirror_Fuse();
   
            string mountPoint = $"/tmp/NeoFS";
            System.Console.WriteLine($"Mounting filesystem at {mountPoint}");

            Fuse.LazyUnmount(mountPoint);

            // Ensure mount point directory exists
            Directory.CreateDirectory(mountPoint);

            try
            {
                var mo = new MountOptions
                {
                    SingleThread = false
                };

                using (var mount = Fuse.Mount(mountPoint, fileSystem, mo))
                {
                    await mount.WaitForUnmountAsync();
                }
            }
            catch (FuseException fe)
            {
                Console.WriteLine($"Fuse throw an exception: {fe}");

                Console.WriteLine("Try unmounting the file system by executing:");
                Console.WriteLine($"fuser -kM {mountPoint}");
                Console.WriteLine($"sudo umount -f {mountPoint}");
            }
        }
    }
}