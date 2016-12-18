using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RCI
{
    using System;
    using System.Runtime.InteropServices;

    namespace RCI
    {
        public class RemoteWindowsNetworking
        {
            #region Consts

            private const int RESOURCE_CONNECTED = 0x00000001;
            private const int RESOURCE_GLOBALNET = 0x00000002;
            private const int RESOURCE_REMEMBERED = 0x00000003;

            private const int ANY = 0x00000000;
            private const int DISK = 0x00000001;
            private const int PRINT = 0x00000002;

            private const int GENERIC = 0x00000000;
            private const int DOMAIN = 0x00000001;
            private const int SERVER = 0x00000002;
            private const int SHARE = 0x00000003;
            private const int FILE = 0x00000004;
            private const int GROUP = 0x00000005;

            private const int CONNECTABLE = 0x00000001;
            private const int CONTAINER = 0x00000002;


            private const int CONNECT_INTERACTIVE = 0x00000008;
            private const int CONNECT_PROMPT = 0x00000010;
            private const int CONNECT_REDIRECT = 0x00000080;
            private const int CONNECT_UPDATE_PROFILE = 0x00000001;
            private const int CONNECT_COMMANDLINE = 0x00000800;
            private const int CONNECT_CMD_SAVECRED = 0x00001000;

            private const int CONNECT_LOCALDRIVE = 0x00000100;

            // Create a constant with a 32bits
            private const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;

            #endregion

            #region Errors

            private const int NO_ERROR = 0;

            private const int ERROR_ACCESS_DENIED = 5;
            private const int ERROR_ALREADY_ASSIGNED = 85;
            private const int ERROR_BAD_DEVICE = 1200;
            private const int ERROR_BAD_NET_NAME = 67;
            private const int ERROR_BAD_PROVIDER = 1204;
            private const int ERROR_CANCELLED = 1223;
            private const int ERROR_EXTENDED_ERROR = 1208;
            private const int ERROR_INVALID_ADDRESS = 487;
            private const int ERROR_INVALID_PARAMETER = 87;
            private const int ERROR_INVALID_PASSWORD = 1216;
            private const int ERROR_MORE_DATA = 234;
            private const int ERROR_NO_MORE_ITEMS = 259;
            private const int ERROR_NO_NET_OR_BAD_PATH = 1203;
            private const int ERROR_NO_NETWORK = 1222;

            private const int ERROR_BAD_PROFILE = 1206;
            private const int ERROR_CANNOT_OPEN_PROFILE = 1205;
            private const int ERROR_DEVICE_IN_USE = 2404;
            private const int ERROR_NOT_CONNECTED = 2250;
            private const int ERROR_OPEN_FILES = 2401;
            private const int ERROR_PATH_NOT_FOUND = 53;

            private const int ERROR_DEV_NOT_EXIST = 0x37;


            private struct ErrorClass
            {
                public int num;
                public string message;

                public ErrorClass(int num, string message)
                {
                    this.num = num;
                    this.message = message;
                }
            }


            // Created with excel formula:
            // ="new ErrorClass("&A1&", """&PROPER(SUBSTITUTE(MID(A1,7,LEN(A1)-6), "_", " "))&"""), "
            private static ErrorClass[] ERROR_LIST = new ErrorClass[]
            {
            new ErrorClass(ERROR_ACCESS_DENIED, "Error: Access Denied"),
            new ErrorClass(ERROR_ALREADY_ASSIGNED, "Error: Already Assigned"),
            new ErrorClass(ERROR_BAD_DEVICE, "Error: Bad Device"),
            new ErrorClass(ERROR_BAD_NET_NAME, "Error: Bad Net Name"),
            new ErrorClass(ERROR_BAD_PROVIDER, "Error: Bad Provider"),
            new ErrorClass(ERROR_CANCELLED, "Error: Cancelled"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_INVALID_ADDRESS, "Error: Invalid Address"),
            new ErrorClass(ERROR_INVALID_PARAMETER, "Error: Invalid Parameter"),
            new ErrorClass(ERROR_INVALID_PASSWORD, "Error: Invalid Password"),
            new ErrorClass(ERROR_MORE_DATA, "Error: More Data"),
            new ErrorClass(ERROR_NO_MORE_ITEMS, "Error: No More Items"),
            new ErrorClass(ERROR_NO_NET_OR_BAD_PATH, "Error: No Net Or Bad Path"),
            new ErrorClass(ERROR_NO_NETWORK, "Error: No Network"),
            new ErrorClass(ERROR_BAD_PROFILE, "Error: Bad Profile"),
            new ErrorClass(ERROR_CANNOT_OPEN_PROFILE, "Error: Cannot Open Profile"),
            new ErrorClass(ERROR_DEVICE_IN_USE, "Error: Device In Use"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_NOT_CONNECTED, "Error: Not Connected"),
            new ErrorClass(ERROR_OPEN_FILES, "Error: Open Files"),
            new ErrorClass(ERROR_PATH_NOT_FOUND, "Error: Path Not Found"), 
            new ErrorClass(ERROR_DEV_NOT_EXIST, "Error: The specified network resource or device is no longer available"),
            };

            private static string GetErrorForNumber(int errNum)
            {
                foreach (ErrorClass er in ERROR_LIST)
                {
                    if (er.num == errNum) return er.message;
                }
                return "Error: Unknown, " + errNum;
            }

            #endregion

            #region types

            private enum RESOURCE_SCOPE
            {
                CONNECTED = 0x00000001,
                GLOBALNET = 0x00000002,
                REMEMBERED = 0x00000003,
                RECENT = 0x00000004,
                CONTEXT = 0x00000005
            }

            private enum RESOURCE_TYPE
            {
                ANY = 0x00000000,
                DISK = 0x00000001,
                PRINT = 0x00000002,
                RESERVED = 0x00000008,
            }

            private enum RESOURCE_USAGE
            {
                CONNECTABLE = 0x00000001,
                CONTAINER = 0x00000002,
                NOLOCALDEVICE = 0x00000004,
                SIBLING = 0x00000008,
                ATTACHED = 0x00000010,
                ALL = (CONNECTABLE | CONTAINER | ATTACHED),
            }

            private enum RESOURCE_DISPLAYTYPE
            {
                GENERIC = 0x00000000,
                DOMAIN = 0x00000001,
                SERVER = 0x00000002,
                SHARE = 0x00000003,
                FILE = 0x00000004,
                GROUP = 0x00000005,
                NETWORK = 0x00000006,
                ROOT = 0x00000007,
                SHAREADMIN = 0x00000008,
                DIRECTORY = 0x00000009,
                TREE = 0x0000000A,
                NDSCONTAINER = 0x0000000B
            }


            [StructLayout(LayoutKind.Sequential)]
            private class NETRESOURCE
            {
                public RESOURCE_SCOPE dwScope = 0;
                public RESOURCE_TYPE dwType = 0;
                public int dwDisplayType = 0;
                public RESOURCE_USAGE dwUsage = 0;
                
                public string lpLocalName = "";
                public string lpRemoteName = "";
                public string lpComment = "";
                public string lpProvider = "";
            }


            [StructLayout(LayoutKind.Sequential)]
            private class LPNETRESOURCE
            {
                public RESOURCE_SCOPE dwScope = 0;
                public RESOURCE_TYPE dwType = 0;
                public int dwDisplayType = 0;
                public RESOURCE_USAGE dwUsage = 0;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpLocalName = "";
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpRemoteName = "";
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpComment = "";
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpProvider = "";
            }


            #endregion

            #region externs

            [DllImport("Mpr.dll")]
            private static extern int WNetUseConnection(
               IntPtr hwndOwner,
               NETRESOURCE lpNetResource,
               string lpPassword,
               string lpUserID,
               int dwFlags,
               string lpAccessName,
               string lpBufferSize,
               string lpResult
               );
            
            /// <summary>
            /// Cancels a connection currently added or used
            /// </summary>
            /// <param name="lpName"></param>
            /// <param name="dwFlags"></param>
            /// <param name="fForce"></param>
            /// <returns></returns>
            [DllImport("Mpr.dll")]
            private static extern int WNetCancelConnection2(
                string lpName,
                int dwFlags,
                bool fForce
                );

            /// <summary>
            /// Enumerates through the Shares at a certain DNS/NETBIOS location
            /// </summary>
            /// <param name="ServerName"></param>
            /// <param name="level"></param>
            /// <param name="bufPtr"></param>
            /// <param name="prefmaxlen"></param>
            /// <param name="entriesread"></param>
            /// <param name="totalentries"></param>
            /// <param name="resume_handle"></param>
            /// <returns></returns>
            [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
            private static extern int NetShareEnum(
             StringBuilder ServerName,
             int level,
             ref IntPtr bufPtr,
             uint prefmaxlen,
             ref int entriesread,
             ref int totalentries,
             ref int resume_handle
             );

            /// <summary>
            /// Starts the enumeration operation that enumerates through 
            /// network resources based on given settings
            /// </summary>
            /// <param name="dwScope"></param>
            /// <param name="dwType"></param>
            /// <param name="dwUsage"></param>
            /// <param name="lpNetResource"></param>
            /// <param name="lphEnum"></param>
            /// <returns></returns>
            [DllImport("mpr.dll")]
            private static extern int WNetOpenEnum(
               RESOURCE_SCOPE dwScope,
               RESOURCE_TYPE dwType,
               RESOURCE_USAGE dwUsage,
               [MarshalAs(UnmanagedType.AsAny)][In]
                object lpNetResource,
               ref IntPtr lphEnum);


            /// <summary>
            /// Interface for enumerating through Network resources. This call is 
            /// preceded by WNetOpenEnum
            /// </summary>
            /// <param name="hEnum"></param>
            /// <param name="lpcCount"></param>
            /// <param name="lpBuffer"></param>
            /// <param name="lpBufferSize"></param>
            /// <returns></returns>
            [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
            public static extern int WNetEnumResource(
                IntPtr hEnum,
                ref int lpcCount,
                IntPtr lpBuffer,
                ref int lpBufferSize);


            /// <summary>
            /// Closes the Net Resource Enumeration
            /// </summary>
            /// <param name="hEnum">Pointer to the enumerator</param>
            /// <returns>Returns result code - 0 for success</returns>
            [DllImport("mpr.dll", CharSet = CharSet.Auto)]
            public static extern int WNetCloseEnum(IntPtr hEnum);


            [DllImport("Netapi32.dll", SetLastError = true)]
            static extern int NetApiBufferFree(IntPtr Buffer);

            /// <summary>
            /// Struct that holds the share resource information
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SHARE_INFO_2
            {
                [MarshalAs(UnmanagedType.LPWStr)]
                public string shi2_netname;
                public uint shi2_type;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string shi2_remark;
                public uint shi2_permissions;
                public uint shi2_max_users;
                public uint shi2_current_users;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string shi2_path;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string shi2_passwd;
            }

            #endregion




            public static string ConnectToRemote(string remoteUnc, string username, string password)
            {
                NETRESOURCE nr = new NETRESOURCE
                {
                    dwType = RESOURCE_TYPE.DISK,
                    lpRemoteName = remoteUnc
                };

                int ret = WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);

                if (ret == NO_ERROR) return null;
                return GetErrorForNumber(ret);
            }

            public static string DisconnectRemote(string remoteUnc)
            {
                int ret = WNetCancelConnection2(remoteUnc, CONNECT_UPDATE_PROFILE, false);
                if (ret == NO_ERROR) return null;
                return GetErrorForNumber(ret);
            }

            /// <summary>
            /// A Windows WNET method for iterating through network resources. At the moment this is
            /// not needed, but it seems that non SMB shares cannot be discovered with NetShareEnum
            /// so leaving this here for further testing purposes 
            /// </summary>
            /// <param name="resource">A LPNETRESOURCE object that should be null on initial call</param>
            /// <returns>Returns a string with the error message</returns>
            private static string DiscoverShares(object resource)
            {
                int bufferSz = 32768;
                int shares = -1;
                IntPtr lpEnum = new IntPtr();
                IntPtr lpBuffer = Marshal.AllocHGlobal(bufferSz);
                int offset = Marshal.SizeOf(typeof(LPNETRESOURCE));

                try
                {
                    int ret = WNetOpenEnum(RESOURCE_SCOPE.CONNECTED, RESOURCE_TYPE.ANY, 0, resource,
                        ref lpEnum);

                    if (NO_ERROR != ret)
                        return GetErrorForNumber(ret);

                    ret = WNetEnumResource(lpEnum, ref shares, lpBuffer, ref bufferSz);

                    if (NO_ERROR != ret )
                        return GetErrorForNumber(ret);

                    Console.WriteLine($"Found {shares} Resources");

                    int i = 0;
                    while (i++ < shares)
                    {
                        LPNETRESOURCE lclResource = (LPNETRESOURCE)Marshal.PtrToStructure(lpBuffer, typeof(LPNETRESOURCE));

                        if (lclResource.dwUsage == RESOURCE_USAGE.CONTAINER)
                        {
                            Console.WriteLine("Discovered a container...");
                            DiscoverShares(lclResource);
                        }

                        Console.WriteLine($"NAME: {lclResource.lpRemoteName}. COMM: {lclResource.lpProvider}");


                        lpBuffer += offset;
                    }

                    return null;
                }
                catch (Exception exp)
                {
                    throw new ApplicationException(exp.Message);
                }
            }


            /// <summary>
            /// Enumerates through a given path if the connection succeeded. This is typically used when
            /// a Connection can be established but the path cannot be reached and thus is could be the root
            /// of the file share. In this case it iterates through the Network Resource and returns a list
            /// of the Remote Paths of the the discovered shares relative to the original share path
            /// </summary>
            /// <param name="path">DNS or NETBIOS name</param>
            /// <param name="shares">An out property to hold the discovered shares</param>
            /// <param name="level"></param>
            /// <returns></returns>
            public static string EnumerateRemoteUncConnection(string path, out string[] shares, int level = 2)
            {
                int entriesread = 0, totalentries = 0, resumeHandle = 0;
                int offset = Marshal.SizeOf(typeof(SHARE_INFO_2));
                IntPtr bufPtr = IntPtr.Zero;
                StringBuilder server = new StringBuilder(path);

                int ret = NetShareEnum(server, level, ref bufPtr, MAX_PREFERRED_LENGTH, ref entriesread, ref totalentries, ref resumeHandle);

                if (ret == NO_ERROR)
                {
                    shares = new string[entriesread];

                    IntPtr currentPtr = bufPtr;

                    Console.WriteLine($"Discovered {entriesread} Shares at [{path}]");

                    for (int i = 0; i < entriesread; i++)
                    {
                        SHARE_INFO_2 shi = (SHARE_INFO_2) Marshal.PtrToStructure(currentPtr, typeof(SHARE_INFO_2));

                        if (!shi.shi2_netname.EndsWith("$"))
                            shares[i] = shi.shi2_netname;

                        currentPtr = new IntPtr(currentPtr.ToInt32() + offset);
                    }
                    NetApiBufferFree(bufPtr);
                }
                else
                {
                    shares = new string[0];
                    return GetErrorForNumber(ret);
                }
                return null;
            }
        }
    }


}
