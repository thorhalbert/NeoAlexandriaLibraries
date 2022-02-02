using NeoScry;
using PenguinSanitizer;
using System.Text;
using NeoAssets.Mongo;
using NeoRepositories.Mongo;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using Logos.Utility;
using MongoDB.Bson;

namespace NeoCli;


public partial class Program
{
    public class NodeMark
    {
        public IMongoCollection<NeoVirtFS> NeoVirtFSCol { get; }

        private FileNode m;
        private int level;
        public byte[] Name { get; private set; }

        private ObjectId namespaceId;
        private ObjectId nodeId;

        private ObjectId rootOfVolume;



        public bool Broken { get; set; }

        public NodeMark(FileNode m, int level, NodeMark[] stack, ObjectId rootOfVolume, IMongoDatabase db)
        {
            NeoVirtFSCol = db.NeoVirtFS();

            this.m = m;
            this.level = level;

            Name = m.Name.ToArray();
            this.rootOfVolume = rootOfVolume;

            var v = new NeoVirtFS
            {
                Name = Name
            };

            if (stack == null)
            {
                v.NARPImport = v.Name.GetString();
                Console.WriteLine($"NARP Import {v.NARPImport}");

                var vol = NeoVirtFSVolumesCol.FindSync(x => x._id == rootOfVolume).FirstOrDefault();
                if (vol == null)
                    throw new Exception($"Can't find volume {v.NARPImport}");

                // Make sure everything exists - this should make the filesystem nodes

                namespaceId = vol.NameSpace;
                nodeId = vol.NodeId;

                v.VolumeId = rootOfVolume;
                return;
            }
            else
            {
                v.NARPImport = stack[0].Name.GetString();
                //Console.WriteLine($"NARP Import {v.NARPImport}");

                v.VolumeId = rootOfVolume;
            }

            // Need potential uuid5 handle - build our raw full path

            var realPath = new List<byte>(Encoding.ASCII.GetBytes("/NARP"));
            foreach (var p in stack)
            {
                realPath.Add((byte) '/');
                realPath.AddRange(p.Name);
            }

            // Work the contents

            Broken = true;


            if (m.IsLnk)
            {
                var link = m.SymbolicLink.GetString();

                if (link.StartsWith(AssetTag))
                {
                    var sha1 = link.Substring(AssetTag.Length);

                    realPath.Add((byte) '/');
                    realPath.AddRange(m.Name.ToArray());

                    var fileuuid = GuidUtility.Create(GuidUtility.UrlNamespace, realPath.ToArray());

                    var theFilter = Builders<AssetFiles>.Filter.Eq("_id", fileuuid);
                    var baRec = af.FindSync(theFilter).FirstOrDefault();
                    if (baRec == null)
                    {
                        Console.WriteLine($"Can't find {Encoding.UTF8.GetString(realPath.ToArray())}");
                        return;
                    }
                    else
                    {
                        baRec.CheckStat(); // Recover old format stat

                        //Console.WriteLine($"Found baRec {baRec._id}");
                        v.Stat = new NeoVirtFSStat(baRec.Stat).ToFile();

                        // We've got to check the asset to see if it's actually annealed -- it could be lost
                        // If we're a mirror we'll skip it

                        var assFilter = Builders<BakedAssets>.Filter.Eq("_id", sha1);
                        var ass = bac.FindSync(assFilter).FirstOrDefault();

                        if (ass == null)
                        {
                            Console.WriteLine($"Can't find the asset");
                            return;   // Should handle as 'lost' node
                        }

                        if (!ass.Annealed.Value)
                        {
                            Console.WriteLine($"Asset is not annealed - so that file is lost -- skipping");
                            return;    // Should handle as 'lost' node
                        }


                    }

                    // If we get to here, we have an annealed asset type, so we need to process
                    Broken = false;

                    var parentId = EvalFullPath(level, stack, rootOfVolume);

                    var getFilter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, parentId) &
                     Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.Name, m.Name);

                    var node = NeoVirtFSCol.FindSync(getFilter).FirstOrDefault();
                    if (node != null)
                    {
                        // Node already exists, need to update it 
                        nodeId = node._id;

                        // Skip for now
                        return;
                    }

                    v = NeoVirtFS.CreateNewFile(parentId,
                        rootOfVolume,
                        m.Name.ToArray(), 
                        null, 
                        v.Stat.st_mode.GetMode(),
                        NeoVirtFSContent.AnnealedAsset(Convert.FromHexString(sha1)));

                    Console.WriteLine($"Create Asset: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())} Level={level}  Parent={parentId} Id={v._id} SHA1={sha1}");

                    NeoVirtFSCol.InsertOne(v);

                    return;
                }
                else
                {
                    // Just going to skip it for now - have to look at the different link types
                    // The one we probably want to handle is purgecontainer links
                    Console.WriteLine($"Unknown link {link}");
                    return;
                }
            }
            else if (m.IsDir)
            {
                var parentId = EvalFullPath(level, stack, rootOfVolume);

                //Console.WriteLine($"Parent: {parentName} {parentId}");

                var getFilter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, parentId) &
                      Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.Name, m.Name);

                var node = NeoVirtFSCol.FindSync(getFilter).FirstOrDefault();
                if (node != null)
                {
                    // Node already exists, need to update it 
                    nodeId = node._id;

                    if (node._id == parentId)
                        throw new ApplicationException($"Looping directories: {Encoding.UTF8.GetString(realPath.ToArray())}");


                    throw new ApplicationException($"Directory duplicated: {Encoding.UTF8.GetString(realPath.ToArray())} {Encoding.UTF8.GetString(m.Name.ToArray())}");

                    // Skip for now
                    return;
                }

                v = NeoVirtFS.CreateDirectory(parentId, rootOfVolume, m.Name.ToArray(), m.FileStat.st_mode);
                nodeId = v._id;

                if (v._id == parentId)
                    throw new ApplicationException($"Looping directories 2: {Encoding.UTF8.GetString(realPath.ToArray())}");

                Console.WriteLine($"Create Directory: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())} Level={level}  Parent={parentId} Id={nodeId}");

                NeoVirtFSCol.InsertOne(v);

                Broken = false;
                return;
            }
            else if (m.IsFile)
            {
                Console.WriteLine($"Encountered File: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())}");
            }
            else
            {
                Console.WriteLine($"Unknown File: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())}");
            }



        }

        private ObjectId EvalFullPath(int level, NodeMark[] stack, ObjectId rootOfVolume)
        {
            var parentId = rootOfVolume;
            var parentName = "(root-volume)";

            if (level > 0)
            {
                StringBuilder sb = null;
                foreach (var s in stack.Skip(1))
                {
                    if (sb != null)
                        sb.Append(" / ");
                    else
                        sb = new StringBuilder();

                    sb.Append(Encoding.UTF8.GetString(s.Name));
                }

                if (sb != null)
                {
                    //Console.WriteLine($"Stack Path: {sb.ToString()}");
                    parentName = sb.ToString();
                }

                var curPar = rootOfVolume;

                foreach (var s in stack.Skip(1))
                {
                    // These could be cached - it would speed things up quite a bit

                    var nodeF = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, curPar) &
                        Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.Name, s.Name);

                    var pRec = NeoVirtFSCol.FindSync(nodeF).FirstOrDefault();
                    if (pRec == null)
                        throw new Exception($"Can't find element {curPar} {Encoding.UTF8.GetString(s.Name)}");

                    curPar = s.nodeId;
                }

                parentId = curPar;
            }

            return parentId;
        }
    }
}
