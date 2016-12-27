using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCI
{
    public class FileInfo
    {
        public string Name { get; set; }

        public string DirectoryPath { get; set; }

        public string Extension { get; set; }

        public FileInfo(string path, WIN32_FIND_DATA win32FindData)
        {

            DirectoryPath = path;
            Name = win32FindData.cFileName;
            Extension = Path.GetExtension(win32FindData.cFileName);

        }
    }

    public class DirectoryInfo
    {
        public string Path { get; set; }

        public DirectoryInfo(string path, WIN32_FIND_DATA win32FindData)
        {
            ChildDirectories = new List<DirectoryInfo>();
            Files = new List<FileInfo>();
            Path = path;
            Name = win32FindData.cFileName;
        }

        public string ParentPath { get; set; }

        public string Name { get; set; }

        public DirectoryInfo ParentDirectory { get; set; }

        public IList<DirectoryInfo> ChildDirectories { get; set; }
        public IList<FileInfo> Files { get; set; }
    }
}
