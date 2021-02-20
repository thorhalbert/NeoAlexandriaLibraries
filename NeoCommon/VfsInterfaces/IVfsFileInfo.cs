using System;
using Tmds.Linux;

namespace NeoCommon.VfsInterfaces
{
    public interface IVfsFileInfo : IVfsCommon
    {
        IVfsPath Filename { get; set; }

        int Open(IVfsPath path);
        void Release(IVfsPath path);

        /// <summary>
        /// Seekability appearently is a private concern of this method (if offset rewinds then it may need
        /// to restart at beginning, or keep a cache --- this just for baked files).  
        /// </summary>
        /// <param name="path"></param>
        /// <param name="offset"></param>
        /// <param name="buffer"></param>
        /// <param name="fi"></param>
        /// <returns></returns>        
        int Read(IVfsPath path, ulong offset, Span<byte> buffer);
        int Create(IVfsPath path, mode_t mode);
        int Truncate(IVfsPath path, ulong length);
        int Write(IVfsPath path, ulong offset, ReadOnlySpan<byte> buffer);
    }
}