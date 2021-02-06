using DokanNet;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;

namespace NeoDokan_AssetFs
{
    public class Dokan_FileProvider : IDokanOperations
    {
        IFileProvider baseFs;
        public Dokan_FileProvider(IFileProvider fs)
        {
            baseFs = fs;
        }

        #region IDokanOperations Implementation
        void IDokanOperations.Cleanup(string fileName, IDokanFileInfo info)
        {
            
        }

        void IDokanOperations.CloseFile(string fileName, IDokanFileInfo info)
        {
           
        }

        NtStatus IDokanOperations.CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            return NtStatus.AccessDenied;
        }

        NtStatus IDokanOperations.DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            return NtStatus.AccessDenied;
        }

        NtStatus IDokanOperations.DeleteFile(string fileName, IDokanFileInfo info)
        {
            return NtStatus.AccessDenied;
        }

        NtStatus IDokanOperations.FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = new List<FileInformation>();

            throw new NotImplementedException();

            return NtStatus.Success;
        }

        NtStatus IDokanOperations.FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            throw new NotImplementedException();

            return NtStatus.Success;
        }

        NtStatus IDokanOperations.FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            throw new NotImplementedException();

            return NtStatus.Success;
        }

        NtStatus IDokanOperations.FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        NtStatus IDokanOperations.GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        NtStatus IDokanOperations.Mounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        NtStatus IDokanOperations.MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            return NtStatus.AccessDenied;
        }

        NtStatus IDokanOperations.ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.AccessDenied;
        }

        NtStatus IDokanOperations.SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            return NtStatus.AccessDenied;
        }

        NtStatus IDokanOperations.SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            return NtStatus.AccessDenied;
        }

        NtStatus IDokanOperations.UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus IDokanOperations.Unmounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        NtStatus IDokanOperations.WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            bytesWritten = 0;
            return NtStatus.AccessDenied;
        }
        #endregion
    }
}
