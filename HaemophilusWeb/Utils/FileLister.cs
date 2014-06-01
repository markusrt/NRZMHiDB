using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaemophilusWeb.Utils
{
    public class FileLister
    {
        private readonly string path;
        private readonly string fileExtension;

        public FileLister(string path, string fileExtension)
        {
            if (!fileExtension.StartsWith("."))
            {
                throw new ArgumentException("File extension should start with a '.'", "fileExtension");
            }
            this.path = path;
            this.fileExtension = fileExtension;
        }

        public List<FileInfo> Files
        {
            get { return FindFiles(); }
        }

        private List<FileInfo> FindFiles()
        {
            return !Directory.Exists(path)
                ? new List<FileInfo>() 
                : Directory.EnumerateFileSystemEntries(path, "*" + fileExtension, SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).ToList();
        }
    }
}