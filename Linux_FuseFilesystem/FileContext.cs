using System;
using System.Collections.Generic;
using System.Text;

namespace Linux_FuseFilesystem
{
    public class FileContext
    {
        public Guid? ExtFileHandle { get; set; }
        public string ExtAssetSha1 { get; set; }
        public AssetFileSystem.AssetFile.UnbakeForFuse AssetLink { get; set; }
    }
}
