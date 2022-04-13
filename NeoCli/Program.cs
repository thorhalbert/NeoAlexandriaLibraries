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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Confluent.Kafka;
using NeoCliFunctions;

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


    [Verb("scry", HelpText = "Determine metadata for files")]
    class ScryOptions
    { //normal options here
    }
    [Verb("anneal", HelpText = "Convert physical files to virtual-baked")]
    class AnnealOptions
    { //normal options here
        [Value(0)]
        public IEnumerable<string> Targets { get; set; }

        [Option('a', "assetdebug", Required = false,  HelpText = "Turn on debugging in asset decoder")]
        public bool AssetDebug { get; set; }
    }
    [Verb("bake", HelpText = "Generate baked file components")]
    class BakeOptions
    { //normal options here
    }
    [Verb("import-narp", HelpText = "Import an existing NARP")]
    class ImportNARPOptions
    { //normal options here
        [Value(0)]
        public IEnumerable<string> NarpList { get; set; }
        [Option('s', "skipifdone", Required = false, HelpText = "Skip if the file is already assimilated")]
        public bool SkipIfDone { get; set; }

        [Option('a', "archivemode", Required = false, HelpText = "Assimilate Archives")]
        public bool ArchiveMode { get; set; }


    }
    [Verb("dump-physical", HelpText = "Dump the backup of a physical NARP")]
    class DumpNARPOptions
    { //normal options here
        [Value(0)]
        public IEnumerable<string> NarpList { get; set; }
    }

    [Verb("server", HelpText = "Test Server")]
    class ServerOptions
    { //normal options here
        //[Value(0)]
        //public IEnumerable<string> NarpList { get; set; }
    }

    static int Main(string[] args)
    {
        db = NeoMongo.NeoDb;

        af = db.AssetFiles();
        bac = db.BakedAssets();      
        bvol = db.BakedVolumes();

        NeoVirtFSVolumesCol = db.NeoVirtFSVolumes();

        var typesL = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToList();

        var types = GetVerbs.GetVerbInAssembly(typesL);

        Parser.Default.ParseArguments(args, types)
              .WithParsed(Run_Command)
              .WithNotParsed(HandleErrors);

       
        return 0;
    }

    private static void HandleErrors(IEnumerable<CommandLine.Error> obj)
    {
        foreach (var v in obj)
            Console.WriteLine($"Error: {v.Tag}");
    }

    private static void Run_Command(object obj)
    {
        switch (obj)
        {
            case ScryOptions c:
                Console.WriteLine("Command scry");
                break;
            case AnnealOptions o:
                //Console.WriteLine("Command anneal");
                AnnealTargets(db, o.Targets, o.AssetDebug);
                break;
            case BakeOptions a:
                Console.WriteLine("Command bake");
                break;
            case ImportNARPOptions a:
                //Console.WriteLine("Command import-narp");
                foreach (var narp in a.NarpList)
                    AssimilateNarp(narp, a);
                break;
            case DumpNARPOptions a:
                //Console.WriteLine("Command dump-narp");
                foreach (var narp in a.NarpList)
                    DumpNARP(narp);
                break;
            case ServerOptions s:
                procServer();
                break;

            default:
                if (obj is ICommandCallable call)
                {
                    // Load it up
                    call.db = db;

                    // Run it
                    call.RunCommand();
                }
                else throw new ArgumentException("Unknown/Unimplemented Arguments");

                break;
        }
        Console.WriteLine("");
    }

    private static void procServer()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "cirdan:9092",
            GroupId = "test",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<string, string>(config).Build())
        {
            var topics = new List<string>();
            topics.Add("neofiles");
            consumer.Subscribe(topics);

            CancellationTokenSource cts = new CancellationTokenSource();
           
            try
            {
                while (true)
                {
                    try
                    {
                        var cr = consumer.Consume(cts.Token);

                        Console.WriteLine($"Consumed message {cr.Key}/{cr.Value} at: '{cr.TopicPartitionOffset}'.");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occurred: {e.Error.Reason}");

                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                consumer.Close();
            }     

            consumer.Close();
        }
    }

    private static void AnnealTargets(IMongoDatabase db, IEnumerable<string> targets, bool assetDebug)
    {
        // cfe1f8f2-65fa-5f28-b1d5-d337648658fc_61ff2a33540d6fe877a01ca3
        // 000000000111111111122222222223333333333444444444455555555556666666666
        // 1234567890123456789012345678901234567890123456789012345678901234567890

        foreach (var t in targets)
        {
            var obj = "";

            var tar = t.Split('/');
            var pth = tar.Last();
            if (pth.Length == 61)
            {
                obj = pth.Substring(37);
                NeoAssets.Mongo.NeoVirtFS.DoAnneal(db, obj, t, assetDebug);
            }
            else
                NeoAssets.Mongo.NeoVirtFS.DoAnneal(db, t, null, assetDebug);
        }
    }

    private static void DumpNARP(string narp)
    {
        NeoVirtFSVolumes.EnsureNarpVolumeExists(db, narp);

        // Just the beginning path (maybe we'll support the leading / but we're doing the normalized version)

        var volume = $"NARP/{narp}";

        if (narp.StartsWith("Mirror", StringComparison.InvariantCulture) ||
            narp.StartsWith("IA-", StringComparison.InvariantCulture))
            volume = $"MIRRORS/{narp}";

        //var rootOfVolume = NeoVirtFS.EnsureVolumeSetUpProperly(db, volume);

        Console.WriteLine($"Scan Physical: {narp}");
        //Console.WriteLine($"Volume Root: {rootOfVolume}");

        var scan = ScanFileDirectory.RecursiveScan($"/NARP/{narp}/".ToSpan(), narp.ToSpan());
        dumpPhysical(scan, narp);
    }

    private static void AssimilateNarp(string narp, ImportNARPOptions a)
    {
        if (a.SkipIfDone && isMarkAssimilated(narp))
        {
            Console.WriteLine($"NARP: {narp} already assimilated");
            return;
        }

        NeoVirtFSVolumes.EnsureNarpVolumeExists(db, narp);

        // Just the beginning path (maybe we'll support the leading / but we're doing the normalized version)

        var volume = $"NARP/{narp}";

        if (narp.StartsWith("Mirror", StringComparison.InvariantCulture) ||
            narp.StartsWith("IA-", StringComparison.InvariantCulture))
            volume = $"MIRRORS/{narp}";

        var rootOfVolume = NeoAssets.Mongo.NeoVirtFS.EnsureVolumeSetUpProperly(db, volume);

        Console.WriteLine($"Scan Physical: {narp}");
        Console.WriteLine($"Volume Root: {rootOfVolume}");

        Console.WriteLine($"Assimilate/Import NARP {narp} - map to {volume} - Volume Id {rootOfVolume}");

        var scan = ScanFileDirectory.RecursiveScan($"/NARP/{narp}/".ToSpan(), narp.ToSpan());
        Console.WriteLine("Loaded: Assimilating");

        Assimilate(scan, 0, null, rootOfVolume);

        MarkAssimilated(narp);

        dumpPhysical(scan, narp);
    }

    private static void dumpPhysical(FileNode scan, string narp)
    {
        var path = $"/ua/NeoVirtContentBaks/Physical-{narp}-{DateTime.Now.ToString("yyyy-MM-dd")}.json";
           
        string jsonString = JsonSerializer.Serialize(scan, options: new JsonSerializerOptions { 
            WriteIndented=true,
            NumberHandling=JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition=JsonIgnoreCondition.WhenWritingDefault});

        File.WriteAllText(path, jsonString);

        Console.WriteLine($"[Write Physical Dump: {path}]");
    }

    private static void MarkAssimilated(string narp)
    {
        var path = $"/ua/NeoVirtContentBaks/Physical-{narp}.import-narp-done";

        string dateString = $"{DateTime.Now}";

        File.WriteAllText(path, dateString);
    }

    private static bool isMarkAssimilated(string narp)
    {
        var path = $"/ua/NeoVirtContentBaks/Physical-{narp}.import-narp-done";
        return File.Exists(path);
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
        if (scan.Members != null)
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
