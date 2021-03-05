using System;
using System.Collections.Generic;
using System.Text;
using Tmds.Fuse;

namespace Linux_FuseFilesystem
{
    public class FuseMountableBase : FuseFileSystemBase
    {
        internal ReadOnlySpan<byte> TransformPath(ReadOnlySpan<byte> path)
        {
            return path;
        }


        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::GetXAttr()");
            return base.GetXAttr(path, name, data);
        }
        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::ListXAttr()");
            return base.ListXAttr(path, list);
        }
        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::SetXAttr()");
            return base.SetXAttr(path, name, data, flags);
        }
        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            path = TransformPath(path);

            Console.WriteLine($"NeoFS::RemoveXAttr()");
            return base.RemoveXAttr(path, name);
        }
    }
}
