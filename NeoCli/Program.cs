
using CommandLine;
using NeoScry;
using PenguinSanitizer;
using System.Reflection;
using System.Text;
using System.Management.Automation;

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
            var verbs = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();

            Parser.Default.ParseArguments(args, verbs)
                .WithParsed<AssimilateOptions>(options => { })
                .WithParsed<ScryOptions>(options => { })
                .WithParsed<AnnealOptions>(options => { })
                .WithParsed<BakeOptions>(options => { })
                .WithNotParsed(errors => { });

            var start = "/rj/NARPS/Mirrors/Mirrors-bitsavers";
            Console.WriteLine($"Try to do a scan of {start}");
            var scan = ScanFileDirectory.RecursiveScan(start.ToSpan(), "Mirrors-bitsavers".ToSpan());

            dump(scan, 0);



            return 0;
        }

        private static void dump(FileNode scan, int level)
        {
            var ind = "";
            for (var i = 0; i < level; i++)
                ind += "    ";

            Console.WriteLine($"{ind}{scan.Name.GetString()} {info(scan)}");
            foreach (var m in scan.Members)
                dump(m, level + 1);
        }

        private static string info(FileNode scan)
        {
            if (scan.IsFile)
                return $"{scan.FileStat.st_size}";
            if (scan.IsLnk)
                return $" link={scan.SymbolicLink.GetString()}";

            return "";
        }
    }
}