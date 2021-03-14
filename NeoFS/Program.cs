using Linux_FuseFilesystem;
using MongoDB.Driver;
using NeoRepositories.Mongo;
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
            var conn = Environment.GetEnvironmentVariable("MONGO_URI");
            var Connection = new MongoClient(conn);

            var NeoDb = Connection.GetDatabase("NeoAlexandria");

            var narps = NeoDb.NARPs();
                 
            if (!Fuse.CheckDependencies())
            {
                Console.WriteLine(Fuse.InstallationInstructions);
                return;
            }

            var provisioner = new ProvisionFilesystem();


         

            var fileSystem = provisioner.CreateFs(NeoDb, narps);
   
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