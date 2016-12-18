using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCI
{
    public class FsFile
    {

        private string _ParentPath { get; set; }

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Extension { get; set; }

        public long Size { get; set; }

        public FsFolder Folder { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string Version { get; set; }

        public string ParentPath
        {
            get
            {
                return GetParentPath();
            }
            set { SetParentPath(value); }
        }

       
        public Guid? SharedBy { get; set; }
        
        public FsFile()
        {
            CreatedOn = DateTime.MaxValue;
            UpdatedOn = DateTime.MaxValue;
        }

        public FsFile(string path, WIN32_FIND_DATA data)
        {
            Name = data.cFileName;
            Extension = GetExtenstion();
            Size = (((long)data.nFileSizeHigh) << 0x20) | data.nFileSizeLow;
            ParentPath = path;
            Guid = Guid.NewGuid();
        }


        public string PermissionString { get; set; }

        public string GetAbsolutePath()
        {
            return ParentPath.StartsWith("\\")
                ? ($@"\{ParentPath.Replace("\\\\", "\\")}\{Name}")
                : ($@"{ParentPath}\{Name}").Replace("\\\\", "\\");
        }

        private string GetParentPath()
        {
            return _ParentPath;
        }

        private void SetParentPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
                _ParentPath = path.StartsWith("\\")
                    ? "\\" + path.Replace("\\\\", "\\").TrimEnd('\\')
                    : path.Replace("\\\\", "\\").TrimEnd('\\');
        }

        private string GetExtenstion()
        {
            if (Name.Contains("."))
                return "." + Name.Split('.')[1];

            //TODO: This should not happen, but permit it for now
            return " ";
        }
    }

    public class FsFolder
    {
        public IList<FsFolder> SubFolders { get; set; }

        private string _ParentPath { get; set; }

        public Guid Guid { get; set; }

        public IList<FsFile> Files { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public bool IsLocked { get; set; }

        public string Root { get; set; }

        public string Name { get; set; }

        public int FileCount { get; set; }

        public string ParentPath
        {
            get { return GetParentPath(); }
            set { SetFullPath(value); }
        }


        public FsFolder ParentFolder { get; set; }

        public Guid? SharedBy { get; set; }

        public bool IsOnRoot()
        {
            return ParentFolder == null;
        }

        public string GetAbsolutePath()
        {
            return ParentPath.StartsWith("\\")
                ? ($@"\{ParentPath.Replace("\\\\", "\\")}\{Name}")
                : ($@"{ParentPath}\{Name}").Replace("\\\\", "\\");
        }

        public FsFolder()
        {
            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
            Guid = Guid.NewGuid();
            SubFolders = new List<FsFolder>();
            Files = new List<FsFile>();

        }

        public FsFolder(string path, WIN32_FIND_DATA data)
        {
            Name = data.cFileName;
            ParentPath = path;
            Guid = Guid.NewGuid();
            SubFolders = new List<FsFolder>();
            Files = new List<FsFile>();
        }

        public string PermissionString { get; set; }

        public bool HasChildren()
        {
            FileCount = Files?.Count ?? 0;

            return SubFolders.Any() || this.FileCount > 0;
        }

        private string GetParentPath()
        {
            return _ParentPath;
        }

        private void SetFullPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
                _ParentPath = path.StartsWith("\\")
                    ? "\\" + path.Replace("\\\\", "\\").TrimEnd('\\')
                    : path.Replace("\\\\", "\\").TrimEnd('\\');
        }

        public bool IsPersonal()
        {
            Guid dummy;
            return Guid.TryParse(Name, out dummy);
        }

    }
}
