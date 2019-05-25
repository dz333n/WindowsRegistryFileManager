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

        public delegate void FileAddedHandler(RegFiles sender, string fileName);
        public event FileAddedHandler FileAdded;

        public delegate void FileRemovedHandler(RegFiles sender, string fileName);
        public event FileRemovedHandler FileRemoved;

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
            FileAdded?.Invoke(this, fileName);
        }

        public void AddFileAsync(FileInfo localFile)
        {
            new Thread(() =>
            {
                byte[] buffer = FileToBuffer(localFile);
                AddFile(localFile.Name, buffer);
            }).Start();
        }

        public void AddFileAsync(string fileName, byte[] buffer) => new Thread(() => AddFile(fileName, buffer)).Start();

        public byte[] FileToBuffer(FileInfo file) => File.ReadAllBytes(file.FullName);

        public void WriteFileAsync(string fileName, string localPath) => new Thread(() => WriteFile(fileName, localPath)).Start();

        public void WriteFile(string fileName, string localPath) => File.WriteAllBytes(localPath, GetFile(fileName));

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
                FileAdded?.Invoke(this, file);
        }

        public RegFiles(RegistryKey key) => Key = key;
    }
}
