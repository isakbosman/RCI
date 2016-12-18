using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace RCI
{


    public static class FileSystemObjectReader
    {
        public static IEnumerable<FsFolder> Read(string path, string seatchpattern, Action<int> status)
        {
            return new FileSystemReader(path, seatchpattern).ReadRootDirectory(status);
        }
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    public class FileSystemReader
    {
        internal enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }

        internal enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories = 1,
            FindExSearchLimitToDevices = 2
        }


        public const int FIND_FIRST_EX_LARGE_FETCH = 0x00000002;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeHandle FindFirstFileEx(
        string lpFileName,
        FINDEX_INFO_LEVELS fInfoLevelId,
        [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATA lpFindFileData,
        FINDEX_SEARCH_OPS fSearchOp,
        IntPtr lpSearchFilter,
        int dwAdditionalFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextFile(SafeHandle hFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        static extern uint GetFullPathName(string lpFileName, uint nBufferLength, [Out] StringBuilder lpBuffer, out StringBuilder lpFilePart);

        private readonly Dictionary<string, FsFolder> _directoryData;
        private readonly string _pattern;
        private readonly string _currentpath;
        private int _objectsFound;
        private readonly List<Task> _parallelTasks;
        private const int _parallelDepthLimit = 1000;
        private readonly int _parallelDepth;

        public FileSystemReader(string path, string searchPattern)
        {
            _currentpath = path;
            _pattern = searchPattern;
            _directoryData = new Dictionary<string, FsFolder>();
            _parallelTasks = new List<Task>();
            _parallelDepth = 0;
        }

        public void ReadDirectory(string path, Action<int> status, FsFolder folder = null)
        {
            bool result = true;

            WIN32_FIND_DATA _win32FindData = new WIN32_FIND_DATA();
            SafeHandle safeFileHandle = null;

            while (result)
            {
                if (safeFileHandle == null)
                {
                    string _path = Path.Combine(path, _pattern);

                    safeFileHandle = FindFirstFileEx(_path, FINDEX_INFO_LEVELS.FindExInfoBasic, _win32FindData,
                        FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

                    result = !safeFileHandle.IsInvalid;
                }
                else
                {
                    result = FindNextFile(safeFileHandle, _win32FindData);
                }

                // Break here cause it continues even though the result is false
                if (!result) continue;

                if ((_win32FindData.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // Ignore the current and parent directory
                    if (!string.IsNullOrEmpty(_win32FindData.cFileName) && !_win32FindData.cFileName.StartsWith("."))
                    {

                        var subfolder = new FsFolder(path, _win32FindData);
                        string fullFolderPath = Path.Combine(subfolder.ParentPath, subfolder.Name);

                        if (folder != null)
                        {
                            subfolder.ParentFolder = folder;
                            folder.SubFolders.Add(subfolder);
                        }

                        _directoryData.Add(fullFolderPath, subfolder);

                        if (_parallelDepth < _parallelDepthLimit)
                            _parallelTasks.Add(Task.Factory.StartNew(() => { ReadDirectory(fullFolderPath, status, subfolder); }, TaskCreationOptions.AttachedToParent));
                        else
                            ReadDirectory(fullFolderPath, status, subfolder);
                    }
                }
                else
                {
                    folder?.Files.Add(new FsFile(path, _win32FindData));
                }
                _objectsFound++;
                status(_objectsFound);
            }
            safeFileHandle.Dispose();
            _win32FindData = null;
        }

        public IEnumerable<FsFolder> ReadRootDirectory(Action<int> status)
        {
            try
            {
                _parallelTasks.Add(Task.Factory.StartNew(() => { ReadDirectory(_currentpath, status); }));

                Task.WaitAll(_parallelTasks.ToArray());

                return _directoryData.Values.AsParallel().AsEnumerable();
            }
            catch (AggregateException ae)
            {
                throw new Exception(ae.Message);
            }
        }
    }

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), BestFitMapping(false)]
    public class WIN32_FIND_DATA
    {
        public FileAttributes dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    [SecurityCritical]
    public sealed class SafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll")]
        private static extern bool FindClose(IntPtr handle);


        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeHandle()
                : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return FindClose(handle);
        }
    }
}
