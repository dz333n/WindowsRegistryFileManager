using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RegistryFileManager
{
    public class RegFiles
    {
        public RegistryKey Key;

        public delegate void FileAddHandler(RegFiles sender, string fileName);
        public event FileAddHandler FileAddStart;
        public event FileAddHandler FileAddEnd;

        public delegate void FileRemovedHandler(RegFiles sender, string fileName);
        public event FileRemovedHandler FileRemoved;

        public delegate void FileWriteHandler(RegFiles sender, string fileName, string localPath);
        public event FileWriteHandler FileWriteStart;
        public event FileWriteHandler FileWriteEnd;

        public string[] GetFileNames() => Key.GetValueNames();

        public byte[] GetFile(string fileName) => (byte[])Key.GetValue(fileName);

        public void DeleteFile(string fileName)
        {
            Key.DeleteValue(fileName);
            FileRemoved?.Invoke(this, fileName);
        }

        public void AddFile(string fileName, byte[] buffer)
        {
            Key.SetValue(fileName, buffer);

            FileAddEnd?.Invoke(this, fileName);
        }

        public void AddFileAsync(FileInfo localFile)
        {
            new Thread(() =>
            {
                FileAddStart?.Invoke(this, localFile.Name);

                byte[] buffer = FileToBuffer(localFile);
                AddFile(localFile.Name, buffer);
            }).Start();
        }

        public void AddFileAsync(string fileName, byte[] buffer) => new Thread(() => AddFile(fileName, buffer)).Start();

        public byte[] FileToBuffer(FileInfo file) => File.ReadAllBytes(file.FullName);

        public void WriteFileAsync(string fileName, string localPath) => new Thread(() => WriteFile(fileName, localPath)).Start();

        public void WriteFile(string fileName, string localPath)
        {
            FileWriteStart?.Invoke(this, fileName, localPath);

            File.WriteAllBytes(localPath, GetFile(fileName));

            FileWriteEnd?.Invoke(this, fileName, localPath);
        }

        public void MoveFileFromRegistry(string fileName, string localPath)
        {
            WriteFile(fileName, localPath);
            DeleteFile(fileName);
        }

        public void DeleteAllFiles()
        {
            foreach (var file in GetFileNames())
                DeleteFile(file);
        }

        public void Start()
        {
            foreach (var file in GetFileNames())
            {
                FileAddEnd?.Invoke(this, file);
            }
        }

        public RegFiles(RegistryKey key) => Key = key;
    }
}
