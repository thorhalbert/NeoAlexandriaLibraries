using Linux_FuseFilesystem;
using MongoDB.Driver;
using NeoCommon;
using NeoRepositories.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.Fuse;
using Tmds.Linux;

namespace NeoFS
{
    public class ProvisionFilesystem
    {
        // Get the symbolic link for the value - what we physically mount to
        //private unsafe byte[] getLink(byte[] narp)
        //{
        //    ssize_t retl;
        //    var buffer = new byte[1024];

        //    fixed (byte* b = buffer)
        //    fixed (byte* n = narp)
        //    {
        //        retl = LibC.readlink(n, b, buffer.Length);
        //    }

        //    if (retl < 1) return new byte[0];

        //    var link = new byte[retl];

        //    Array.Copy(buffer, link, retl);
        //    buffer = null;

        //    return link;
        //}
        //public IEnumerable<KeyValuePair<byte[], byte[]>> GetNarpsOld()
        //{
        //    var dh = RawDirs.opendir(RawDirs.ToNullTerm(Encoding.UTF8.GetBytes("/NARP/")));

        //    while (true)
        //    {
        //        var dir = RawDirs.readdir_wrap(dh);
        //        if (dir == null) break;

        //        var d = dir.Value;

        //        var narp = new List<byte>(Encoding.ASCII.GetBytes("/NARP/"));
        //        narp.AddRange(d.d_name);

        //        var link = getLink(narp.ToArray());
        //        if (link.Length > 0)
        //            yield return new KeyValuePair<byte[], byte[]>(d.d_name, link);
        //    }

        //    RawDirs.closedir(dh);

        //    yield break;
        //}

      

        public IEnumerable<KeyValuePair<byte[], byte[]>> GetNarps(IMongoCollection<NARPs> narps)
        {
            var theFilter = Builders<NARPs>.Filter.Empty;

            var cursor = narps.Find(theFilter).ToList();
            foreach (var n in cursor)
                yield return new KeyValuePair<byte[], byte[]>(
                    Encoding.ASCII.GetBytes(n._id),
                    Encoding.ASCII.GetBytes(n.LastPhysical));


            yield break;
        }

        // The mount-mapper really should run every 15 minutes or so
        internal IFuseFileSystem CreateFs(IMongoDatabase db, IMongoCollection<NARPs> bac)
        {
            var rootSystem = new NarpMirror_TopLevel();
            var basePassthrough = new NarpMirror_Fuse();

            // Now we have to generate all the mountpoints

            var topLayer = new NarpMirror_MountDispatch(rootSystem);

            // This gets the links from /NARP - maybe this should come from mongo NARPs (or a mixture)
            foreach (var v in GetNarps(bac))
            {
                var narp = new List<byte>(Encoding.ASCII.GetBytes("/"));   // /NARP/
                narp.AddRange(v.Key);

                topLayer.Mount(v.Key.AsSpan(), narp.ToArray().AsSpan(), v.Value.AsSpan(), basePassthrough);

                var name = Encoding.UTF8.GetString(v.Key);
                var link = Encoding.UTF8.GetString(v.Value);
                Console.WriteLine($"Mount {name} as {link}");
            }

            // Load the mountpoints
            rootSystem.Mountpoints = topLayer.MapSys;

          


            return topLayer;
        }
    }
}
