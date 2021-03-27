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

            // Test to see if we can new a FuseFileInfo

            var fi = new FuseFileInfo();

            var provisioner = new ProvisionFilesystem();




            //var fileSystem = new NarpMirror_Fuse();  //  provisioner.CreateFs(NeoDb, narps);
            var fileSystem =  provisioner.CreateFs(NeoDb, narps);
            //var fileSystem = new Mounter.MemoryFileSystem(); 

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

                // Debug = "-d"
                using (var mount = Fuse.Mount(mountPoint, fileSystem, mo, Arguments : new string[] {"-d", "-o", "allow_other" }))
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