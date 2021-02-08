# Assimilation System 
Replacing all of the Python

This system makes heavy use of Kafka and changes the nature of assimilation from the NARP down to the 
directory and it's files.   This gets around a lot of the existing assimilation issues which tend
to cause the same processes to run endlessly on the same files and makes it nearly impossible to
fully assimilation huge NARPs since when they fail the system has to start over again at the 
beginning.   So, basically this changes everything to be event driven at the file level.

This is not unlike the current way the NARPProcessing system works except that it happens at the file
level.   

Theoretically this system will allow us to scale out endlessly (and eventually kubernetify everything), 
without having to have tons of disks mounted to the docker containers (which wasn't particularly
maintainable).

## What we are Assimilating

There are four main levels of this.

1. NARP (Neo-Alexandria Repositories) - these are directories containing the actual original content.
They represent curation.  That is, they have files that are related in some way.   I giant bag of hashes isn't
that interesting.   They've been put together in some fasion that permits someone to browse them
in some order.   It doesn't matter if the same file is repeated throughout -- these are eventually
deduplicated.   The databases which track this data are handled by the 'NeoRepositories' library.

    We also want a way for this to run on A) Remote machines, or B) Windows/Non-Linux machines.  We
    would likely have a new way to register NARPs on remote systems, and a windows service that would
    run periodically to perform these operations.  You could have it, say, upload the current status
    of your Documents directory every night at 2am.  You could then conceivably time travel through
    different versions of a file which have existed over time.   The 'BakedVolume' philosophy is to 
    'never delete'.   Though maybe we'll have the ability to 'tombstone' a file which you want to become
    inaccessible (though it will likely always exist)--it would be up to high level security levels
    to enforce it.

1. Assets - these are the individual files, which are tracked by their SHA1 (there are a ton of other
hashes calculated, but the SHA1 is the key -- when I started this, SHA1 was secure enough, and I don't
see any reason to change this).   There is a primary asset database which has metadata, history,
status attached to it.  Assets *are* a disembodied bag of hashes.   Assets are '*Scryed*', which is
the kind of file they are is determined and metadata is collected for that filetype.  This is all
stored on the asset.

1. BakedVolumes - Assets can either exist on the disk as a real file or then can be 'baked'.  These
is basically contained in a giant tar file file split up to form a RAID6 architecture.   Baked
archives are essentially a series of split of tar files that are built in this fashion:  
   * Asset is compressed with gzip -9, and a file with the current asset info is generated.   
   * These are assembled into a work directory until they are about 4 GB in size.
   * The large tar file is split evenly into 4 files (Baked file Parts A-D)
   * A fifth XOR (parity) file is created (The X part).   
   * A sixth ECC is generated with PAR2 (the Z part) with 25% reduncancy on the A-D parts.
   * For a given volume no 'part' is ever permitted on the same disk platter.   The theory
 is that you can lose 2 disks.   I've never actually needed to use the ECC so I don't actually
have a recovery system for this yet.   It would theoretically be able to perform bit level recovery
(while the parity disk allows file level recovery).
   * Once the volumes are made -- they are *Annealed*.   Annealing consists of the following:
     * The entire volume is walked and hashes are computed to make sure the assembly process has been
faithful.  
     * Once this is ensured an asset is marked 'Annealed'.   
     * Files on disk are hashed to check
that the file actually represents what the metadata says they are -- if they match, then a posix
symbolic link replaces the file (there's supposed to be a virtual filesystem to actually make that
work, but that's not functional at this time -- hopefully we can build a next generation version of 
that in .net -- the python fuse version was never particullarly reliable).
     * Live NARPs (mostly mirrors) are not Annealed (which is a pain because then you tend to have to
have 2 or more copies of all these files -- one baked and one physical).  If we can come up with a 
good virtual filesystem for the NARPs, then we can run rsync an wget against it and let the underlying
files anneal and save a lot of net filespace.

1. The new system we want to build is the Curated Collections system.    This basically allows
either an autocuration system or one where some can browse the NARPs, or employ a search engine
(ElasticSearch) and manually assemble their own collections.   This would be extroverted as a new
virtual filesystem.    Also assets, or groups of assets (directory full of pictures, or several separate
chapters, a pdf file with a cover in a separate jpg) can be assembled into an '*Artifact*'.
An Artifact is an object which attempts to model the object virtually and is intended to operate
in a Virtual Reality system.   Artifacts model physical books, 3 ring binders, tapes (to represent archives, 
or literal tapes which can be mounted on a mainframe tape drive simulation), Music CDs or Vinyl Albums which can be placed
in a stereo and played.

## Use Cases for Assimilation

### Assimilate Directory - General Directory Queue

Walk a directory and generate entries for all the files (keep a list) - to General File Queue.   Then walk through
the recorded directories in the database for the same directory and submit the ones that have
not already been sent.   Also, send subdirectories recursively to the General Directory Queue.

Note that files may/may not be annealed, so you're just sending the link.   Also we may or may not
have a SHA1 for this file.

Check files/directories for issues (mostly that they're readable by the user running the service) - we 
might submit instead to a repair queue.

We might dispatch to a directory 'shape' analyzer.   Look for all of the files of an audiobook - a
music album, a large stack of jpg files that are a deck of photos or a raw book.   Find books, covers for
books, collections.    There are probably operations that will then happen at the level of collections
of files (which would likely be an Artifact).   Might look for all five parts of a multipart rar file.

### Assimilate File - General File Queue

This is the general work dispatcher.   Things can get delegated and then delegated back until we're
finished.

Determine if we have a SHA1 for this file - if not, we will compute and resubmit.   See if we have
a physical copy of the file (a cached unbaked for or the real file).   This is just noted.   A 
delegated service will cause this to be retrieved if necessary.   However, we might update the
cache timeouts for the file if we see it to keep it online.

We will do the following:
 * Get the stat for the file - generate a stateuuid - if we're getting info for a file that's no longer
in existence, then we mark the appopriate places farther down
 * Get metadata if we're baked
 * Upsert 'AssetDirs' and 'AssetFiles' in mongo and the 'FileInfo' in sql
 * If this is a new file, create a sub 'Asset' - at least add the nominal file extension to the list
of file extensions in the type.
 * Get our Metadata Staleness data from the Asset and prepare to dispatch to the extractors

Not clear if we just do this ourselves, or we really do have a pile of microservices each with their own 
queue doing this work (the file ping-pong around --- or worse getting put back at the end of a queue 
when the cached files and such have to sit around and possibly go stale waiting isn't the best plan).

When data is fully processed there might be some process to roll information back up to the 
directory -- ultimately this can be rolled up to the NARP.

We will want this to work on windows machines.   We might have the ability to push files onto the server
from the windows directories.  It's unlikely that we'd permit annealing of the windows directories.

#### ScryFile

Determine the FileType (a guid heirarchy).

If stale dispatch to metadata collector for this FileType.  PDF file stats, epub metadata, multimedia
audio/visual metadata, bitmap size, etc.   We may even eventually go as far as to extract archive subdirectories
and tape catalogs.

System might also suggest an initial artifact type if the Asset seems to be a self contained piece of
content.

#### Metadata Extractors

These collect data specific to a particular FileType and live in a MetaData polymorphism.
Metadata extractors will likely need a physical file.  This might call a file unbaker and wait
to be called again.

#### Cover Extractors/Finders

These either find the cover in another asset file, or they extract them.   From a PDF, or epub,
or possible from an mp3 tag.   This might use Imagemagick Convert or some such to do it.

Cover extraction will likely need an online file.

#### Archive/Compression Extractors

Explodes and archive out into it's component files, which are then separately Assimilated.
ISO, ZIP, RAR, LHZ, ZOO, TAR, XZ, Z, BZ2, GZ, TPC, TAP.   We may even support disk images in
different formats (an ISO is like this, but we could for example have a FAT floppy disk, or a 
DEC RSTS/E RP06).   Tape formats are containers, and might have filesytem formats inside them, like 
ANSI or DOS.

There's a complex ecosystem here, between container formats.   The idea is, that if encounter
any type of archive file or asset, you can drill into it, or convert it into another format.

#### Archive Purgers

If the Policy allows it may be permitted that a the original archive has something that looks
like annealing happen, where it gets converted from a file to a directory and has a link to the
Extract directory on it.   However, it is sometimes important that the original file is important,
like a zip which has firmware in it and the system demands that the zip file exists with a certain
hash.  Reversible extracts aren't really a thing yet (like FLAC bitwise compressions of music which
would generate the same bits as the input) -- you can't get reverse a ZIP file to regenerate the same
bits.   Still, keeping the original archive can double the amount of total storage, so routine
Policy might permit Container Purging.

Not likely to initially run on remote windows servers.

#### File Annealing

Unlike archives, Baked files will generate the same bits, and so are safer to anneal.  Though there
are some environments, like having an NARP which contains an rsync or wget mirror.   Unmodified
rsync and wget has numerous problems dealing with annealed files (symbolic link to a virtual filesystem).
So, default Mirror policy would likely not permit it.  Someday a virtualized NARP filesystem might
alleviate this.

Similarly would not run on windows.

#### File Baking

This is the initial file collection version of constructing a baked volume.

We may rewrite this to push gzip compressed files up to a .net GRDP service -- this would make it so the
temporarly caches for Baked Volumes would not require the whole NARP filesystem to be visible.

This would also run nicely on windows and allow files to be injected into the volume backing system
from windows or any remote system.   It can also change the order of operations from scry/metadata first
to baking first for these systems.  

This baker then needs to run more like the python baker so it can run at command line or in
a windows service, but can also work a kafka queue.

Also need to figure out what to do with files that have been submitted but have not yet caused
a volume to bake.  The volume has to accumulate 4 gb of files before it triggers, so the intermediate
PREP file might be able to used to generate a cache entry.   With local filesystems the original 
file is available until it
anneals.  With remote systems, these won't be available.   A new level of nuance to track.

### Asset Queue

For some operations, like initial volume build/anneal, we might want to send an Asset SHA1 and have it
explode out into all files for that Asset.

### Unbake

Get a file out of baked storage and recover it into a cache area.

We also might be able to get at submitted but unbaked/annealed PREP files.   

### Unfurl (Decompose Asset/Artifact)

Break into an asset (SHA1) or an artifact (UUID -- stand along Artifact IDs are the UUID for the SHA1).

Unfurling is basically an extended cache directory that would turn, for example, a PDF into 
a series of bitmaps, or turns most any kind of document (epub, rtf, doc) into a PDF file and then
into a series of bitmaps.

This also might be an archive getting exploded (if that hasn't already happened).

This can be selective, where you trying to get at just a few pages, for example to get a quick render
for a cover and back page, though hopefully the cover has already been realized somewhere.

There might be another component to this which then enabled a multimedia file to be streamed.

#### PDFify

Convert to PDF File:  txt, mobi, epub, lit, djvu, rtf, doc, 

#### Archive/Decompression Exploders

For Unfurl and for Archive Extractors

General frame work for extractors.

 * Compression:  Z, gz, bz, bz2, xz, _
 * Archives: zip, zoo, rar, 7z, lzh, cab, jar, tar, cpio, so, a, 
 * Quasi Containers:  iso
 * Backup Formats:  Tops-10/20 Dumper, VAX/VMX BCK
 * Tape containers: TPC, TAP 
   * Tape filesystems:  DOS, RSTS/E, ANSI
 * Floppy Formats (Containers):
   * Floppy Filesystems: Amiga, FAT, DOS
 * Disk Pack Formats:

This might have to call command line tools to operate.

### Retrieve Virtual Document

We have enough metadata from the internet (looking somewhat at Internet Archive here), where we can 
render a facsimile of the potential artifact with metadata and a cover thumbnail, but there's no need to keep this data in our our storage
farm.   Instead, we can download it dynamically and then unfurl it.   The system could optionally
then add it to the storage pools once it's gone through the trouble of downloading it.  Or it can just
keep in cache until unneeded, then delete it (though we can keep the enhanced metadata we now have).

There would eventually be a series of 'actualize' libraries for different kinds of online storage.
Such as get from an S3 share, an Internet Archive file, pull from a general URL.



