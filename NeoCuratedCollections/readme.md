# Design for Curated Collections

This is a system for curated (and perhaps auto-curated) management of digital assets from the 
NeoAlexandria CMS - converting them into appropriate real-world (virtual world) representations, 
called 'Artifacts' (i.e., books, tapes, record, albums, and other abstracted metaphors for truly 
digital/binary items like zip files).

Actual file assets can be aggregated or decomposed -- several files of different chapters can be 
compiled into a single book, while individual pages can be altered, flipped, cropped, rearranged 
as desired.

Similarly individual items can be placed into collections, like books on a shelf, and hundreds 
of books sorted by their title or LoC numbers to form a series of shelves.

Also, these will be packed into large record in mongo, so that we can efficiently load all of 
this metadata into the VR environment.

Initial overarching metaphor is the library bookshelf, where data is measured in book-meters.    

* Artifact Prototype

* Collection (Polymorphism)
  * Artifact with possible Aggregation/Customized Assets
  * Sorted collection of:
    * De-facto implied artifacts (direct asset references)
    * Aggregated/Customized Artifacts

Maybe we want different collections for artifacts vs. collection objects, or we can just have the 
polymorphism.

Artifacts also contain an entity representation from 3dworlds, which would give a 3d representation
and the emmisaries required for it to operate.   The Emissaries will connect to entity servers, which
will be serving the content out.  Books for example (at least at the beginning) will have an api
which can explode the document into a series of png/jpg files which can then be displayed as the page
contents (for various document types) -- eventually they'll be smart enough to see words and do searches
like a regular pdf viewer would do.   Initial graphics will be poor, but eventually we want photorealistic
representations of the artificts.

Eventually there will be a protobuf3 representation of all 3d objects, and the emissary will be 
contained in a cryptographically signed tar file.   There will be an infrastructure to store and serve
these up, but not sure it will have a mongo backend (maybe not the most efficient way to do it) -- have
been looking at possibily an s3 share, but haven't found working code to do that correctly yet.

Let's store guids and sha1's, etc as binary strings.

I'm thinking the Curation database is a flattened heirarchy.   These are persisted as reasonably
large records in mongo for efficiency.

Was also thinking of making things stack oriented (push/pop).  Not sure this is needed -- a set/unset might
be adequate.

Having an integer keyed object would be quick/easy to update, but not as efficient to have 
just an array of objects, but harder to update.

We want this to be rather short in bson.  Keep the names short.
We need a set of backtrack/salvage keys, probably indexed

* _id: sequenced guid in binary
* TopId: binary top level collection
* TopSeq: integer sequence
* Members: array of objects (state machine schema)
  * S - sequence #
  * AR - Artifact Type Object
    * Art: Artifact Key (binary)
    * ...  artifact private parameters
  * CR - Start Collection (binary key)
  * CI - Collection information (string)
  * CS - collection sequence
  * CD - collection depth
  * A - Import asset (binary key)
  * C - Import other collection
  * EA - Enumerate Aggregate Object
    * Array of asset keys or collection keys - asset map
      * Contents array
        * Range 
          * Array [ sourcemap, first, last ]
        * Select 
          * Array [odd,even,...] - page pairs, odd is map index, even is key (either a page #, or a CB? filename)
        * Seq - page # or whatever - allows multiple items on same page - If range, then next X are implied
        * Rotate - 90 degree rotations - 0 up, clockwise
        * Position: x,y
        * Scale: double
        * Flip - flip over axis from angle, 0 up, clockwise
        * Up - original up angle (page that's 90 degress for example) - 0 up, clockwise
        * Crop - Pos + Bounds
        * Clip - clip against polygonal boundary
        * Z - stack order for page
        * Attribs - subobject which has artifact level values (like page #)


