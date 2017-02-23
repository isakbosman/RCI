/* 
 * Copyright (C) 2017 kryptogeek (kryptogeek@privacyrequired.com)
 * All rights reserved.
 *
 * This application is network file system connectivity utility written
 * by Isak Bosman (kryptogeek@privacyrequired.com).
 * 
 */
using System.Collections.Generic;
using System.IO;

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
