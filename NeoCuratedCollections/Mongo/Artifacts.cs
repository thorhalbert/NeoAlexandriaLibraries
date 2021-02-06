using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCuratedCollections.Mongo
{
    /// <summary>
    /// Artifacts are an idea like a mime-type, but it's essentially a heirarchy
    /// of precision:   Unknown/Document/PDF File
    /// PDF implies document, etc.   The Root is always Unknown
    /// 
    /// Also, the idea is that once a guid is assigned then it should be permanently
    /// associated with that document type (monument guid).
    /// 
    /// Eventually there will have to be artifact specific code to determine and
    /// extract metadata for different attribute types.  Libraries like libmagic
    /// unfortunately don't keep metadata and file type separated, so the data has 
    /// to be tweezered apart with some domain specific code.   And there's just idea
    /// of what's metadata and what isn't.   For Document/PDF/PDF 3.4 -- is the version
    /// number metadata or the filetype? (I've decided it's metadata, but it's a 
    /// judgement call).
    /// 
    /// The older Asset/NeoScanAll data has pretty good coverage for the libmagic
    /// and metadata capture for media files and pdf files (and some data that can be
    /// extracted from document formats).   It would be nice to be able to fully 
    /// generate an artifact designation with metadata without having to actually rescan
    /// the bits from the file.
    /// 
    /// We'll make a NeoScry library for this logic.
    /// 
    /// We may compute some kind of 'confidence' value so we know if we want to 
    /// make another scry pass (and force the bits back online, or make it so we can
    /// directly scry from the baked file) 
    /// </summary>
    public class Artifacts
    {
        public Guid _id { get; set; }
        // List of parents (rightmost is the root - so this is never null except for the root itself)
        public Guid[] Parents { get; set; }

    }
}
