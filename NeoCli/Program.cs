
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
using MongoDB.Bson;

namespace NeoCli;


public class ByteMemoryComp : IComparer<ReadOnlyMemory<byte>>
{
    public int Compare(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
    {
        return x.Span.SequenceCompareTo(y.Span);
    }
}

public partial class Program
{
    const string AssetTag = "/NEOASSET/sha1/";

    public static IMongoDatabase db { get; private set; }

    private static IMongoCollection<AssetFiles> af;
    private static IMongoCollection<BakedAssets> bac;
    private static IMongoCollection<BakedVolumes> bvol;

    static Dictionary<string, NeoVirtFSNamespaces> NamespaceNames = new Dictionary<string, NeoVirtFSNamespaces>();
    static Dictionary<ObjectId, NeoVirtFSNamespaces> Namespaces = new Dictionary<ObjectId, NeoVirtFSNamespaces>();
    static NeoVirtFSNamespaces rootNameSpace;

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

        var start = "Mirrors-bitsavers";
        AssimilateNarp(start);


        return 0;
    }

    private static void AssimilateNarp(string narp)
    {
        NeoVirtFSVolumes.EnsureNarpVolumeExists(db, narp);

        // Just the beginning path (maybe we'll support the leading / but we're doing the normalized version)

        var volume = $"MIRRORS/{narp}";

        var rootOfVolume = NeoVirtFS.EnsureVolumeSetUpProperly(db, volume);

        Console.WriteLine($"Scan Physical: {narp}");
        Console.WriteLine($"Volume Root: {rootOfVolume}");

        var scan = ScanFileDirectory.RecursiveScan($"/NARP/{narp}/".ToSpan(),narp.ToSpan());

        Assimilate(scan, 0, null, rootOfVolume);
    }

    private static void Assimilate(FileNode scan, int level, NodeMark[] nodeStack, ObjectId rootOfVolume)
    {
        var newStack = new List<NodeMark>();
        if (nodeStack == null)
            newStack.Add(new NodeMark(scan, -1, null, rootOfVolume, db));
        else
            newStack.AddRange(nodeStack);

       

        var ind = "";
        for (var i = 0; i < level; i++)
            ind += "    ";

        // Cut off the volume name at the beginning
        var nodeList = string.Join(" / ", newStack.Skip(1).Select(x => x.Name.GetString()));

        //Console.WriteLine($"Node: {nodeList}");

        // Console.WriteLine($"{ind}{scan.Name.GetString()} {info(scan)}");
        foreach (var m in scan.Members)
        {
            var stackCopy = newStack.ToArray().ToList();

            stackCopy.Add(new NodeMark(m, level, newStack.ToArray(), rootOfVolume, db));
 
            Assimilate(m, level + 1, stackCopy.ToArray(), rootOfVolume);
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
