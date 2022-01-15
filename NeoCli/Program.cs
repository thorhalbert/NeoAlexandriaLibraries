
using CommandLine;
using NeoScry;
using PenguinSanitizer;
using System.Reflection;
using System.Text;
using System.Management.Automation;
using NeoAssets.Mongo;
using NeoRepositories.Mongo;
using NeoCommon;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using Logos.Utility;
using MongoDB.Bson;

namespace NeoCli
{

    public class ByteMemoryComp : IComparer<ReadOnlyMemory<byte>>
    {
        public int Compare(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
        {
            return x.Span.SequenceCompareTo(y.Span);
        }
    }

    public class Program
    {
        const string AssetTag = "/NEOASSET/sha1/";

        public static IMongoDatabase db { get; private set; }

        private static IMongoCollection<AssetFiles> af;
        private static IMongoCollection<BakedAssets> bac;
       private static IMongoCollection<BakedVolumes> bvol;

        static Dictionary<string, NeoVirtFSNamespaces> NamespaceNames = new Dictionary<string, NeoVirtFSNamespaces>();
      static  Dictionary<ObjectId, NeoVirtFSNamespaces> Namespaces = new Dictionary<ObjectId, NeoVirtFSNamespaces>();
      static  NeoVirtFSNamespaces rootNameSpace;

        public static IMongoCollection<NeoVirtFSVolumes> NeoVirtFSVolumesCol { get; private set; }

        [Verb("assimilate", HelpText = "Add physical directory to neo-virtual-fs")]
        class AssimilateOptions
        { //normal options here
        }
        [Verb("scry", HelpText = "Determine metadata for files")]
        class ScryOptions
        { //normal options here
        }
        [Verb("anneal", HelpText = "Convert physical files to virtual-baked")]
        class AnnealOptions
        { //normal options here
        }
        [Verb("bake", HelpText = "Generate baked file components")]
        class BakeOptions
        { //normal options here
        }


        static int Main(string[] args)
        {
             db = NeoMongo.NeoDb;

            af = db.AssetFiles();
            bac = db.BakedAssets();
            bvol = db.BakedVolumes();

             NeoVirtFSVolumesCol = db.NeoVirtFSVolumes();

            var verbs = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();

            Parser.Default.ParseArguments(args, verbs)
                .WithParsed<AssimilateOptions>(options => { })
                .WithParsed<ScryOptions>(options => { })
                .WithParsed<AnnealOptions>(options => { })
                .WithParsed<BakeOptions>(options => { })
                .WithNotParsed(errors => { });

            var start = "/NARP/Mirrors-bitsavers/";
            Console.WriteLine($"Try to do a scan of {start}");
            var scan = ScanFileDirectory.RecursiveScan(start.ToSpan(), "Mirrors-bitsavers".ToSpan());

            dump(scan, 0, null);



            return 0;
        }

        private class NodeMark
        {
            private FileNode m;
            private int level;
            public byte[] Name { get; private set; }

            private ObjectId namespaceId;
            private ObjectId nodeId;

            public NodeMark(FileNode m, int level, List<NodeMark> stack = null)
            {
                this.m = m;
                this.level = level;

                Name = m.Name.ToArray();

                var v = new NeoVirtFS
                {
                    Name = Name
                };

                if (stack == null)
                {
                    v.NARPImport = v.Name.GetString();
                    //Console.WriteLine($"NARP Import {v.NARPImport}");

                    var vol = NeoVirtFSVolumesCol.FindSync(x => x.Name == v.NARPImport).FirstOrDefault();
                    if (vol == null)
                        throw new Exception($"Can't find volume {v.NARPImport}");

                    // Make sure everything exists - this should make the filesystem nodes
                  
                    NeoVirtFS.PullNamespacesAndVolumes(db, ref NamespaceNames, ref Namespaces,ref rootNameSpace);

                    namespaceId = vol.NameSpace;
                    nodeId = vol.NodeId;

                    v.NameSpace = vol.NameSpace;
                }
                else
                {
                    v.NARPImport = stack[0].Name.GetString();
                    //Console.WriteLine($"NARP Import {v.NARPImport}");

                    v.NameSpace = stack[0].namespaceId;
                }

                // Determine parent



                if (m.IsLnk)
                {
                    var link = m.SymbolicLink.GetString();

                    if (link.StartsWith(AssetTag))
                    {
                        var sha1 = link.Substring(AssetTag.Length);

                        v.Content = new NeoVirtFSContent
                        {
                            ContentType = VirtFSContentTypes.Asset,
                            AssetSHA1 = Convert.FromHexString(sha1),
                        };

                        // Now, we should have the stat somewhere
                        // And we should check to see if this is really annealed or if we've
                        // lost it.  If this is a mirror we stand a chance of getting it back
                        // Kind of a judgement call to just lose the file, or get an EIO error accessing it
                        // At least for mirrors we should just lose it

                        // Need uuid5 handle - build our raw full path

                        var realPath = new List<byte>(Encoding.ASCII.GetBytes("/NARP"));
                        foreach (var p in stack)
                        {
                            realPath.Add((byte) '/');
                            realPath.AddRange(p.Name);
                        }

                        // This gets us to our parent, if we need it

                        //var parentPath = realPath.ToArray();
                        //var paruuid = GuidUtility.Create(GuidUtility.UrlNamespace, parentPath.ToArray());

                        realPath.Add((byte) '/');
                        realPath.AddRange(m.Name.ToArray());
                  
                        var fileuuid = GuidUtility.Create(GuidUtility.UrlNamespace, realPath.ToArray());

                        var theFilter = Builders<AssetFiles>.Filter.Eq("_id", fileuuid);
                        var baRec = af.FindSync(theFilter).FirstOrDefault();
                        if (baRec == null)
                        {
                            Console.WriteLine($"Can't find {Encoding.UTF8.GetString(realPath.ToArray())}");
                            return;
                        }
                        else
                        {
                            baRec.CheckStat(); // Recover old format stat

                            Console.WriteLine($"Found baRec {baRec._id} {baRec.Annealed}");
                            v.Stat = new NeoVirtFSStat(baRec.Stat).ToFile();

                            // We've got to check the asset to see if it's actually annealed -- it could be lost
                            // If we're a mirror we'll skip it

                            var assFilter = Builders<BakedAssets>.Filter.Eq("_id", sha1);
                            var ass = bac.FindSync(assFilter).FirstOrDefault();

                            if (ass == null)
                            {
                                Console.WriteLine($"Can't find the asset");
                                return;
                            }

                            if (!ass.Annealed.Value)
                            {
                                Console.WriteLine($"Asset is not annealed - so that file is lost -- skipping");
                                return;
                            }

                           
                        }
                    }
                    else
                    {
                        // Just going to skip it for now
                        Console.WriteLine($"Unknown link {link}");
                        return;
                    }




                }
                else if (m.IsDir) { }
            }
        }

        private static void dump(FileNode scan, int level, List<NodeMark> nodeStack)
        {
            if (nodeStack == null)
            {
                nodeStack = new List<NodeMark>
                {
                    new NodeMark(scan, -1)
                };
            }
              
            var ind = "";
            for (var i = 0; i < level; i++)
                ind += "    ";

            var nodeList = string.Join(" / ", nodeStack.Select(x => x.Name.GetString()));

            Console.WriteLine($"Node: {nodeList}");

           // Console.WriteLine($"{ind}{scan.Name.GetString()} {info(scan)}");
            foreach (var m in scan.Members) {
                var newNode = new List<NodeMark>(nodeStack)
                {
                    new NodeMark(m, level, nodeStack)
                };
                dump(m, level + 1, newNode);
            }
        }

        private static string info(FileNode scan)
        {
            if (scan.IsFile)
            {
                return $"{scan.FileStat.st_size}";
            }
            if (scan.IsLnk)
                return $" link={scan.SymbolicLink.GetString()}";

            return "";
        }
    }
}