Next generation baked file decoder

1.  Full Async (if we can manage)
2.  Use memory mapped files to get to the different parts of the volume, or cacheing
3.  Utilize the parity file, either to check contents, or recreate a part if one is missing
4.  Reasonable caching - allow users to seek (even though, due to gzip you can't really do it)
5.  Support split files
6.  Binary REDIS for cache info (allow multi-user access to file caches)

We need to find some filesystem or RESTful model for setting up the API.
System to our knowledge is still read-only.

Would eventually be lovely to discover an Async Fuse model.