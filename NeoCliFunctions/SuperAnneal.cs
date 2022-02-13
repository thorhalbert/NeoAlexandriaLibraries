using CommandLine;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCliFunctions
{
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
     

        public void RunCommand()
        {
            Console.WriteLine("Superanneal has been called");
        }
    }

}
