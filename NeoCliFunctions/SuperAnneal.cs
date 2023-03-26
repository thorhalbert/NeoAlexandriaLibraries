using CommandLine;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCliFunctions;

[Verb("superanneal", HelpText = "Convert physical files to virtual-baked")]
public class SuperAnneal : ICommandCallable
{
    public IMongoDatabase db { get; set; }

    [Value(0)]
    public IEnumerable<string> Targets { get; set; }

    [Option('a', "assetdebug", Required = false, HelpText = "Turn on debugging in asset decoder")]
    public bool AssetDebug { get; set; }

    [Option('s', "salvage", Required = false, HelpText = "Recover around deceased Files")]
    public bool Salvage { get; set; }
    public IMongoCollection<BakedAssets> bac { get; private set; }

    public void RunCommand()
    {
        var bvs = db.BakedVolumes();
        bac = db.BakedAssets();

        foreach (var vol in Targets)
        {
            var filter = Builders<BakedVolumes>.Filter.Eq("_id", vol);

            var rec = bvs.FindSync(filter).FirstOrDefault();

            doAnneal(rec);
        }
    }

    private void doAnneal(BakedVolumes rec)
    {
        Console.WriteLine($"[Anneal: {rec._id}]");

        var partList = new Dictionary<int,BakedVolumes_PartValues?>();

        var ps = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
        for (var i = 0; i < ps.Length; i++)
        {
            partList[i] = null;
            if (rec.Parts.ContainsKey(ps[i]))
                partList[i] = rec.Parts[ps[i]];
        }
              
        var skip = false;       // No need to skip unless missing pieces

        for (var i = 0; i < ps.Length; i++)
        {                      
            if (!partList.ContainsKey(i) ||
                !checkPart(partList[i]))
            {
                skip = true;
                continue;
            }

            procPart(partList[i], skip, ps[i], i);

            skip = false;
        }
    }

    private bool checkPart(BakedVolumes_PartValues p)
    {
        var file = Path.Combine(p.Path, p.Name);

        return File.Exists(file);
    }

    ulong RunOffset = 0;
    ulong SkipBlocks = 0;
    private void procPart(BakedVolumes_PartValues p, bool salvageSkip, string part, int parn)
    {
        var file = Path.Combine(p.Path, p.Name);
        Console.WriteLine($"[Open Baked Volume: {file}]");

        ulong offset = 0;

        var io = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
        var ios = io.BaseStream;

        var buffer = new byte[512];

        while (true)
        {
            var r = ios.Read(buffer, 0, buffer.Length);
            if (r < 512)
            {
                Console.WriteLine("[EOF]");
                return;
            }

            offset += 512;
            RunOffset += 512;

            if (SkipBlocks > 0)
            {
                SkipBlocks--;
                continue;
            }

            var header = new TarHeader(buffer, parn, offset, RunOffset);
            if (!header.Valid) continue;
            var size = header.Size;

            SkipBlocks = size / 512;
            if (size % 512 != 0)
                SkipBlocks++;

            //Console.WriteLine($"Header: {header.ToString()}");
            //Console.WriteLine($"Skip: {SkipBlocks}\n");

            checkFile(header, part, offset-512, RunOffset-512, SkipBlocks);
        }
    }
    private void checkFile(TarHeader header, string part, ulong offset, ulong runOffset, ulong skipBlocks)
    {
        var name = Encoding.ASCII.GetString(header.Name);
        //Console.WriteLine($"Name={name}");
        if (!name.EndsWith(".asset.gz")) return;

        var asset = name[2..42];
        var block = runOffset / 512;

     

        var filter = Builders<BakedAssets>.Filter.Eq("_id", asset);

        var rec = bac.FindSync(filter).FirstOrDefault();
        if (rec == null)
        {
            Console.WriteLine($"Can't find Asset: {asset}");
            return;
        }

        var match = true;
        if (part != rec.Part) match = false;
        if (offset != rec.Offset) match = false;
        if (block != rec.Block) match = false;

        if (!match)
            Console.WriteLine($"Asset: {asset} Part: {part}/{rec.Part} Offset: {offset}/{rec.Offset} Block: {block}/{rec.Block} {match}");
    }
}
