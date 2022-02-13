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
        public IMongoCollection<NeoAssets.Mongo.NeoVirtFS> NeoVirtFSCol { get; }
        public IMongoCollection<NeoAssets.Mongo.NeoVirtFS> NeoVirtFSDeletedCol { get; }

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
            NeoVirtFSDeletedCol = db.NeoVirtFSDeleted();

            this.m = m;
            this.level = level;

            Name = m.Name;
            this.rootOfVolume = rootOfVolume;

            var v = new NeoAssets.Mongo.NeoVirtFS
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
                    realPath.AddRange(m.Name);

                    var fileuuid = GuidUtility.Create(GuidUtility.UrlNamespace, realPath.ToArray());

                    var theFilter = Builders<AssetFiles>.Filter.Eq("_id", fileuuid);
                    var baRec = af.FindSync(theFilter).FirstOrDefault();
                    if (baRec == null)
                    {
                        Console.WriteLine($"Can't find assetfile {Encoding.UTF8.GetString(realPath.ToArray())} - using file defaults");
                        v.Stat = NeoVirtFSStat.FileDefault();
                    }
                    else
                    {
                        baRec.CheckStat(); // Recover old format stat

                        //Console.WriteLine($"Found baRec {baRec._id}");
                        v.Stat = new NeoVirtFSStat(baRec.Stat).ToFile();
                    }

                    // We've got to check the asset to see if it's actually annealed -- it could be lost
                    // If we're a mirror we'll skip it

                    var assFilter = Builders<BakedAssets>.Filter.Eq("_id", sha1);
                    var ass = bac.FindSync(assFilter).FirstOrDefault();

                    if (ass == null)
                    {
                        Console.WriteLine($"Can't find the asset - {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name)}");

                        v.Stat.st_size = 0;   // We could conceivably try to look on the Asset for this - restore will need to fix the stat
                        GenerateAssetFile(m, stack, rootOfVolume, v.Stat, sha1, realPath, true);

                        return;   // Should handle as 'lost' node
                    }

                    v.Stat.st_size = ass.FileLength;  // RealLength is the compressed size

                    if (!ass.Annealed.Value)
                    {
                        //Console.WriteLine($"Asset lost -- skipping - {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())}");
                        GenerateAssetFile(m, stack, rootOfVolume, v.Stat, sha1, realPath,true);

                        return;  
                    }
                                   
                    // If we get to here, we have an annealed asset type, so we need to process
                    Broken = false;

                    GenerateAssetFile(m, stack, rootOfVolume, v.Stat, sha1, realPath, false);
                    return;
                }
                else
                {
                    // Just going to skip it for now - have to look at the different link types
                    // The one we probably want to handle is purgecontainer links
                    Console.WriteLine($"Unknown link {link} - {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())}");
                    return;
                }
            }
            else if (m.IsDir)
            {
                GenerateDirectory(m, stack, rootOfVolume, realPath);
                return;
            }
            else if (m.IsFile)
            {
                //Console.WriteLine($"Encountered File: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())}");

                Broken = false;

                GeneratePhysicalFile(m, stack, rootOfVolume, realPath);
                return;
            }
            else
            {
                Console.WriteLine($"Unknown File: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name)}");
            }
        }

        private void GenerateDirectory(FileNode m, NodeMark[] stack, ObjectId rootOfVolume, List<byte> realPath)
        {
            var parentId = EvalFullPath(level, stack, rootOfVolume);

            //Console.WriteLine($"Parent: {parentName} {parentId}");

            var getFilter = Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.ParentId, parentId) &
                  Builders<NeoAssets.Mongo.NeoVirtFS>.Filter.Eq(x => x.Name, m.Name);

            var node = NeoVirtFSCol.FindSync(getFilter).FirstOrDefault();
            if (node != null)
            {
                // Node already exists, no need to update directory
                nodeId = node._id;

                //throw new ApplicationException($"Directory duplicated: {Encoding.UTF8.GetString(realPath.ToArray())} {Encoding.UTF8.GetString(m.Name.ToArray())}");

                // Skip for now
                return;
            }

            var v = NeoAssets.Mongo.NeoVirtFS.CreateDirectory(parentId, rootOfVolume, m.Name, m.FileStat.st_mode);
            nodeId = v._id;

            if (v._id == parentId)
                throw new ApplicationException($"Looping directories 2: {Encoding.UTF8.GetString(realPath.ToArray())}");

            //Console.WriteLine($"Create Directory: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())} Level={level}  Parent={parentId} Id={nodeId}");

            NeoVirtFSCol.InsertOne(v);

            Broken = false;
        }

        private void GeneratePhysicalFile(FileNode m, NodeMark[] stack, ObjectId rootOfVolume, List<byte> realPath)
        {
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

            realPath.Add((byte) '/');
            realPath.AddRange(m.Name.ToArray());

            var v = NeoAssets.Mongo.NeoVirtFS.CreateNewFile(parentId,
                rootOfVolume,
                m.Name,
                null,
                m.FileStat.st_mode,
                NeoVirtFSContent.PhysicalFilePath(realPath.ToArray()));

            //Console.WriteLine($"Create Physical File: {Encoding.UTF8.GetString(realPath.ToArray())}"); // Level={level} Parent={parentId} Id={v._id}");

            NeoVirtFSCol.InsertOne(v);
        }

        private void GenerateAssetFile(FileNode m, NodeMark[] stack, ObjectId rootOfVolume, NeoVirtFSStat stat, string sha1, List<byte> realPath, bool lost)
        {
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
 
            var v = NeoAssets.Mongo.NeoVirtFS.CreateNewFile(parentId,
                rootOfVolume,
                m.Name,
                null,
                stat.st_mode.GetMode(),
                NeoVirtFSContent.AnnealedAsset(Convert.FromHexString(sha1), lost));

            v.Stat = stat;  // Fill in the rest of the stat

            if (lost)
                v.DeleteType = DeleteTypes.LOST;

            var lst = "";
            if (lost)
            {
                lst = "[Lost] ";
            }

            //    Console.WriteLine($"{lst}Create Asset: {Encoding.UTF8.GetString(realPath.ToArray())} / {Encoding.UTF8.GetString(m.Name.ToArray())} SHA1={sha1}");
     
            // Create LOST assets into the deleted collection so it might be possible to ressurect if they later appear

            var procDb = lost ? NeoVirtFSDeletedCol : NeoVirtFSCol;
            procDb.InsertOne(v);

            return;
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
