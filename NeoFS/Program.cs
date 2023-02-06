using Linux_FuseFilesystem;
using MongoDB.Driver;
using NeoRepositories.Mongo;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tmds.Fuse;

namespace NeoFS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
                throw new Exception($"Usage: NeoFS neofs|neovirt mountpoint <fuse-args>");

            var fileType = args[0].ToLowerInvariant();
            var mountPoint = args[1];

            args = args.Skip(2).ToArray();

            var conn = Environment.GetEnvironmentVariable("MONGO_URI");
            var Connection = new MongoClient(conn);

            var NeoDb = Connection.GetDatabase("NeoAlexandria");

            var narps = NeoDb.NARPs();

            if (!Fuse.CheckDependencies())
            {
                Console.WriteLine(Fuse.InstallationInstructions);
                return;
            }

            // Test to see if we can new a FuseFileInfo

            IFuseFileSystem fileSystem;

            switch (fileType)
            {

                case "neofs":
                    throw new Exception("NEOFS deprecated");

                    var provisioner = new ProvisionFilesystem();
                    fileSystem = provisioner.CreateFs(NeoDb, narps);

                    break;

                case "neovirt":
                    fileSystem = new NeoVirtFS.NeoVirtFS(NeoDb);
                    break;

                default:
                    throw new Exception($"Unknown filesystem: {fileType}");
            }
      
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

                // Debug = "-d"
                using (var mount = Fuse.Mount(mountPoint, fileSystem, mo, Arguments : new string[] {"-o", "allow_other" }))
                {
                    System.Console.WriteLine($"Unmounting filesystem at {mountPoint}");
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