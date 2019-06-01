using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RegistryFileManager
{
    public class RegFiles
    {
        public RegistryKey Key;

        // Default handlers

        public delegate void FileOperationBeginHandler(RegFiles sender, string fileName);
        public delegate void FileOperationEndHandler(RegFiles sender, string fileName, Exception exception = null);
        
        public event FileOperationBeginHandler FileAddStart;
        public event FileOperationEndHandler FileAddEnd;
        
        public event FileOperationBeginHandler FileRemoveBegin;
        public event FileOperationEndHandler FileRemoveEnd;

        // Write handlers

        public delegate void FileWriteBeginHandler(RegFiles sender, string fileName, string localPath);
        public delegate void FileWriteEndfHandler(RegFiles sender, string fileName, string localPath, Exception ex = null);

        public event FileWriteBeginHandler FileCopyRegToLocalBegin;
        public event FileWriteEndfHandler FileCopyRegToLocalEnd;

        public event FileWriteBeginHandler FileMoveRegToLocalBegin;
        public event FileWriteEndfHandler FileMoveRegToLocalEnd;

        // Methods

        public string[] GetFiles() => Key.GetValueNames();

        /// <summary>
        /// Get file's content
        /// </summary>
        /// <param name="fileName">File in registry</param>
        public byte[] GetFile(string fileName) => (byte[])Key.GetValue(fileName);

        public int GetFileSize(string fileName) => GetFile(fileName).Length;

        /// <summary>
        /// Delete file from registry
        /// </summary>
        /// <param name="fileName"></param>
        public void DeleteFileAsync(string fileName)
        {
            new Thread(() =>
            {
                FileRemoveBegin?.Invoke(this, fileName);
                try
                {
                    DeleteFile(fileName);
                    FileRemoveEnd?.Invoke(this, fileName);
                }
                catch (Exception ex)
                {
                    FileRemoveEnd?.Invoke(this, fileName, ex);
                }
            }).Start();
        }

        public void FileRenameAsync(string fileName, string newFileName)
        {
            new Thread(() =>
            {
                var content = GetFile(fileName);
                DeleteFileAsync(fileName);
                AddFileAsync(newFileName, content);
            }).Start();
        }

        public void DeleteFile(string fileName)
        {
            Key.DeleteValue(fileName);
        }

        public void AddFile(string fileName, byte[] buffer)
        {
            Key.SetValue(fileName, buffer);
        }

        public void AddFileAsync(FileInfo localFile)
        {
            new Thread(() =>
            {
                FileAddStart?.Invoke(this, localFile.Name);

                try
                {
                    byte[] buffer = FileToBuffer(localFile);
                    AddFile(localFile.Name, buffer);

                    FileAddEnd?.Invoke(this, localFile.Name);
                }
                catch (Exception ex)
                {
                    FileAddEnd?.Invoke(this, localFile.Name, ex);
                }
            }).Start();
        }

        public void AddFilesAsync(string[] files)
        {
            new Thread(() =>
            {
                foreach (var file in files)
                {
                    AddFileAsync(new FileInfo(file));
                //    string name = new FileInfo(file).Name;

                //    FileAddStart?.Invoke(this, name);

                //    byte[] buffer = FileToBuffer(new FileInfo(file));
                //    AddFile(name, buffer);
                }
            }).Start();
        }

        public void AddFileAsync(string fileName, byte[] buffer)
        {
            new Thread(() =>
            {
                FileAddStart?.Invoke(this, fileName);

                try
                {
                    AddFile(fileName, buffer);
                    FileAddEnd?.Invoke(this, fileName);
                }
                catch (Exception ex)
                {
                    FileAddEnd?.Invoke(this, fileName, ex);
                }
            }).Start();
        }

        public byte[] FileToBuffer(FileInfo file) => File.ReadAllBytes(file.FullName);

        /// <summary>
        /// Copy file from registry to hard drive
        /// </summary>
        /// <param name="fileName">Registry file name</param>
        /// <param name="localPath">Local file name</param>
        public void CopyFileFromRegToLocalAsync(string fileName, string localPath)
        {
            new Thread(() => 
            {
                FileCopyRegToLocalBegin?.Invoke(this, fileName, localPath);
                try
                {
                    CopyFileFromRegToLocal(fileName, localPath);

                    FileCopyRegToLocalEnd?.Invoke(this, fileName, localPath);
                }
                catch (Exception ex)
                {
                    FileCopyRegToLocalEnd?.Invoke(this, fileName, localPath, ex);
                }
            }).Start();
        }

        public void CopyFileFromRegToLocal(string fileName, string localPath)
        {
            File.WriteAllBytes(localPath, GetFile(fileName));
        }

        public void MoveFileFromRegToLocalAsync(string fileName, string localPath)
        {
            new Thread(() =>
            {
                FileMoveRegToLocalBegin?.Invoke(this, fileName, localPath);
                try
                {
                    MoveFileFromRegToLocal(fileName, localPath);

                    FileMoveRegToLocalEnd?.Invoke(this, fileName, localPath);
                }
                catch (Exception ex)
                {
                    FileMoveRegToLocalEnd?.Invoke(this, fileName, localPath, ex);
                }
            }).Start();
        }

        public void MoveFileFromRegToLocal(string fileName, string localPath)
        {
            CopyFileFromRegToLocal(fileName, localPath);
            DeleteFileAsync(fileName);
        }

        public void DeleteAllFilesAsync()
        {
            new Thread(() =>
            {
                foreach (var file in GetFiles())
                    DeleteFileAsync(file);
            }).Start();
        }

        public void DeleteAllFiles()
        {
            foreach (var file in GetFiles())
                DeleteFile(file);
        }

        public void Start()
        {
            foreach (var file in GetFiles())
            {
                FileAddEnd?.Invoke(this, file);
            }
        }

        public RegFiles(RegistryKey key) => Key = key;
    }
}
