using System;
using Tmds.Linux;

namespace NeoCommon.VfsInterfaces
{
    public interface IVfsFileInfo : IVfsCommon
    {
        IVfsPath Filename { get; set; }

        int Open();
        void Release();

        /// <summary>
        /// Seekability appearently is a private concern of this method (if offset rewinds then it may need
        /// to restart at beginning, or keep a cache --- this just for baked files).  
        /// </summary>
        /// <param name="path"></param>
        /// <param name="offset"></param>
        /// <param name="buffer"></param>
        /// <param name="fi"></param>
        /// <returns></returns>        
        int Read(ulong offset, Span<byte> buffer);
        int Create(mode_t mode);
        int Truncate(ulong length);
        int Write(ulong offset, ReadOnlySpan<byte> buffer);
    }
}