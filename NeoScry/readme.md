# Scry - Determine Asset Metadata

Three modes - get data from Asset metadata if available.  It can fail down to needing to look at a physical file.
It can need to go out to an baked file.

The key for this is some sort of sieve based on the mime-types,
but we're trying to get around the 35 year old hack for being able to send
binary file attachments in an email.   You can't just dispatch based on hash
for the mimetypes or file extensions, since libmagic might append metadata into the
output with no real generic guide for parsing them out.   And there are different things you can do with the 4 different
things that can come out of the libmagic strings.  Some mime-types are pretty definative though. 
We can start with those.

We want to detect some filetypes that don't have a proper mime-type in libmagic.  Also there are 
several that have metadata embedded in it which have to be parsed and moved.

Try to figure out a confidence estimate?  Like having a 'PDF' filename without a matching PDF mimetype?

It would be nice if this was metadata driven from a collection and having code which was sandboxed
so we can just have a series of plugins for new artifact types, but we'll have to start out with hardcodes.

I had thought of just reimplementing libmagic in c# and make it so it doesn't expose metadata
with the mimetype (which makes it entirely non-determistic for many types---though unfortunatley libmagic is by
far the best tool out there), but that's not really practical.  One advantage there is that we could
just point it at a stream which is the baked file decoder.  But there a few nicely wrapped .net versions of libmagic.





Scry Algorithm (done on a file):

* See if we can determine encoding of filename
* Get some kind of unicode representation of filename (along with the byte array)
* Get stat()/lstat() and compute stateuuid (if physical file)
* See if we're baked (and this is a symbolic link)
* See if this is a link to an (exploded) archive 


* See if we already know this file (get an asset sha1)
  * Look in AssetFiles
  * If no entry or stateuuid mismatch then we have a new file
    * Compute SHA1
      * Checked BakedAssets to see if baked - flags for AssetFiles
    * Archive the old file state in assetfiles if exists
    * fix AssetFiles to current file
    * Upsert AssetDirs
* Get ready to update the SQL
  * Change:
    * New File
    * Asset->Baked (conceivably could go the other way)
    * New Asset - Existing file (file changed stateuuid/hash)
    * Need some process for touching files to see if they're deleted

* Asset setup/Scry
  * Upsert basic info
  * Load Asset