# NeoVirtFS - NeoAlexandria Virtual Filesystem

Unlike my other filesystems, this one has relative rather than absolute.
Individual file nodes look much more like an inode and have random Ids (which I've decided to just use mongo object-ids for speed)

As the name implies, it's virtual, and not intended to be physical.   At the file level, the native file is a baked file, however, new files can be put into physical caches. 
It is also possible to point/map to physical files.

First goal is to be able to replace all physical NARPs with virtual ones.   We're part of the way towards being able to anneal all mirrors for example, though this needs a little work.

But, this is also intended to be able to build curated directories built from NARPs, or whatever.

So, the system has a 'symbolic link' kind of structure, but these are more like mount-points.   
The system is also intended to be composed of hundreds of thousands of self-contained volumes.   These can be mounted at any point and can appear as a file or a directory as appropriate.

Archives/Containers and a new structure called a 'Split File' will become one of these virtual volumes rather than them appearing in the NARPs.   Split files are so we can break up 
large file so they can be baked.

 * NeoVirtFSVolumes - Metadata for individual volumes, including security
 * NeoVirtFSNodes - File/Directory metadata for files in volumes

We want to write a Fuse Filesystem for linux, along with direct access to baked files on local filesystem or via the grpc remote filesystem.
Also thinking about a dokan version (not sure if this will be writable, but would be nice--we might be able to make the grpc filesystem writable).

Volumes
	Type
		NARP					narp$NDP-20211001A
		Container (Archive)		arc$32017917ce1d8883758b9e712e2f5dec35d67e2d$ISO
		Split					split$32017917ce1d8883758b9e712e2f5dec35d67e2d
		User					user$thor$name

Volume names must be valid unicode, whereas paths don't necessarily have to be - these are always simply byte arrays of the raw OS filename.

Dollar might be dicey, but I think it will work in windows and linux.

{
        "_id" : UUID("9a649061-e2af-54d2-9b44-c5ed6641dd36"),
        "Baked" : true,
        "File" : {
                "FileName" : BinData(0,"L05BUlAvTWlycm9ycy1kYml0L2Z0cC5kYml0LmNvbS9wdWIvYWxwaGEvYmxpc3MxMS9SRUFETUU="),
                "CharSet" : "ascii",
                "Extension" : "com/pub/alpha/bliss11/readme",
                "File" : "/NARP/Mirrors-dbit/ftp.dbit.com/pub/alpha/bliss11/README",
                "FileType" : "FILE",
                "NARPId" : UUID("fe5d5b7b-9183-5d94-bd21-7451a2ebd217"),
                "NARP" : "/NARP/Mirrors-dbit",
                "Title" : "Readme"
        },
        "Links" : {
                "AssetLink" : BinData(0,"Yes6iQ989T6V/0MWKIc13qv29+M=")
        },
        "Parent" : UUID("e6a651ea-3a86-5ab1-b535-e6beed1c2788"),
        "SHA1" : BinData(0,"Yes6iQ989T6V/0MWKIc13qv29+M="),
        "Stat" : {
                "uid" : 10010,
                "fileType" : 32768,
                "gid" : 10010,
                "ctime" : 1603594573,
                "mode" : 33188,
                "statTime" : 1619577159.24212,
                "size" : 126,
                "atime" : 1603594573,
                "nlink" : 0,
                "mtime" : 865494000
        }
}


