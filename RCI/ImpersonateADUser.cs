using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RCI
{
    public class ImpersonatedUser
    {
        static IntPtr _userHandle;
        static WindowsImpersonationContext _impersonationContext;

        public static void ImpersonateUser(string user, string domain, string password)
        {
            _userHandle = IntPtr.Zero;
            bool loggedOn = LogonUser(
                user,
                domain,
                password,
                LogonType.Interactive,
                LogonProvider.Default,
                out _userHandle);

            if (!loggedOn)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // Begin impersonating the user
            _impersonationContext = WindowsIdentity.Impersonate(_userHandle);
        }

        public static void ImpersonateUser(string domainUserName, string password)
        {
            if (string.IsNullOrEmpty(domainUserName) || string.IsNullOrEmpty(password))
            {
                return;
            }

            _userHandle = IntPtr.Zero;
            string[] userNameParts = domainUserName.Split('\\');

            string domain = userNameParts[0];
            string user = userNameParts[1];

            bool loggedOn = LogonUser(
                user,
                domain,
                password,
                LogonType.Interactive,
                LogonProvider.Default,
                out _userHandle);

            if (!loggedOn)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // Begin impersonating the user
            _impersonationContext = WindowsIdentity.Impersonate(_userHandle);
        }

        public static void Dispose()
        {
            if (_userHandle != IntPtr.Zero)
            {
                CloseHandle(_userHandle);
                _userHandle = IntPtr.Zero;
                _impersonationContext.Undo();
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            LogonType dwLogonType,
            LogonProvider dwLogonProvider,
            out IntPtr phToken
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        enum LogonType
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            NetworkCleartext = 8,
            NewCredentials = 9,
        }

        enum LogonProvider
        {
            Default = 0,
        }
    }
}
