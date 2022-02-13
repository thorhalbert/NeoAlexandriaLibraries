using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoAssets.Mongo;

namespace NeoScry;

public class ScryContext
{
    public byte[] Sha1Key { get; set; }
    public string Sha1Hex { get; set; }

    public Assets Asset { get; set; }
    public bool AssetDirty { get; set; }

    public string PhysicalName { get; set; }        // Could cause us trouble with messed up filenames
    public byte[] PhysicalRaw { get; set; }
}

