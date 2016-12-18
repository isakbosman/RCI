using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using RCI.RCI;

namespace RCI
{
    class Program
    {
        // Helper method to write a message to the console at the given foreground color.
        internal static void WriteToConsole(ConsoleColor foregroundColor, string format,
            params object[] formatArguments)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;

            Console.WriteLine(format, formatArguments);
            Console.Out.Flush();

            Console.ForegroundColor = originalColor;
        }

        public static bool VerifyAccessRights(string path, params FileSystemRights[] rights)
        {
            if (!Directory.Exists(path))
                return false;

            var access = Directory.GetAccessControl(path);

            var accessRules = access.GetAccessRules(true, true,
                                typeof(System.Security.Principal.SecurityIdentifier));

            foreach (FileSystemAccessRule rule in accessRules)
            {
                foreach (FileSystemRights right in rights)
                {
                    if ((right & rule.FileSystemRights) != right)
                        continue;

                    switch (rule.AccessControlType)
                    {
                        case AccessControlType.Allow:
                            return true;
                        case AccessControlType.Deny:
                            return false;
                    }
                }

            }
            return false;
        }

        static void ThrowInvalidArgumentException()
        {
            throw new Exception(
                        "The parameters are not in the correct format.\n Please supply the following values\n  [-l for Local Share OR -r for Remote Share] [Path To Share] -u [username]");
        }
        static void Main(string[] args)
        {
            string share = "", username = "", password = "";
            bool isLocal;
            bool import = false;

            try
            {
                if (args.Length < 4)
                    ThrowInvalidArgumentException();

                if(!string.Equals("-r", args[0]) && !string.Equals("-l", args[0]))
                    ThrowInvalidArgumentException();


                share = args[1];

                isLocal = string.Equals("-l", args[0]);

                if (args[2] == "-u")
                {

                    username = args[3];

                    if(string.IsNullOrEmpty(username))
                        throw new Exception("The username cannot be empty");

                    Console.WriteLine($"Please enter the password for User: [{username}]");

                    while (true)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.Enter)
                            break;

                        password += key.KeyChar;
                    }
                }

                if (args.Length == 5 && string.Equals(args[4], "-i"))
                    import = true;

                if (isLocal)
                {
                    Console.WriteLine("Connecting to the Local Share...");

                    string[] parts = username.Split('\\');

                    if (parts.Length != 2)
                        throw new Exception("For local share please provide the username as Domain\\Username");

                    ImpersonatedUser.ImpersonateUser(parts[1], parts[0], password);

                    if (Directory.Exists(share))
                    {
                        WriteToConsole(ConsoleColor.Green, "Succesfully connected to the share at: " + share);
                        Console.WriteLine("Verifying Permissions...");

                        WriteToConsole(ConsoleColor.Green, VerifyAccessRights(share, FileSystemRights.CreateFiles) ? $"Sufficient Permissions Exist for User: [{username}]" : $"Permissions Invalid for User: [{username}]");
                        if(import)
                        {
                            int count = 0;
                            Console.WriteLine($"Started import at: { DateTime.Now }");
                            FileSystemObjectReader.Read(share, "*",
                                x => { count = x; });

                            Console.WriteLine($"Completed import at: { DateTime.Now } and read { count } objects");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Connecting to the Remote Share...");

                    var result = RemoteWindowsNetworking.ConnectToRemote(share, username, password);

                    if (string.IsNullOrEmpty(result))
                    {
                        if (Directory.Exists(share))
                        {
                            WriteToConsole(ConsoleColor.Green, "Succesfully connected to the share at: " + share);
                            Console.WriteLine("Verifying Permissions...");

                            WriteToConsole(ConsoleColor.Green, VerifyAccessRights(share, FileSystemRights.CreateFiles) ? $"Sufficient Permissions Exist for User: [{username}]" : $"Permissions Invalid for User: [{username}]");

                            if (import)
                            {
                                int count = 0;
                                Console.WriteLine($"Started import at: { DateTime.Now }");
                                FileSystemObjectReader.Read(share, "*",
                                    x => { count = x; });

                                Console.WriteLine($"Completed import at: { DateTime.Now } and read { count } objects");
                            }
                        }
                        else
                        {
                            WriteToConsole(ConsoleColor.Yellow, "Could not find path. Starting share discovery...");

                            string[] shares;
                            result = RemoteWindowsNetworking.EnumerateRemoteUncConnection(share, out shares);
                            
                            if(!string.IsNullOrEmpty(result))
                                WriteToConsole(ConsoleColor.Red, "Share Discovery Failed. Reason: " + result);

                            if(shares == null || shares.Length == 0)
                                WriteToConsole(ConsoleColor.Blue, "No Shares Found");

                            for (var i = 0; i < shares?.Length; i++)
                            {
                                if (shares[i] != null)
                                {
                                    string sharePath = Path.Combine(share, shares[i]);
                                    if (Directory.Exists(sharePath) &&
                                        VerifyAccessRights(sharePath, FileSystemRights.CreateFiles))
                                        WriteToConsole(ConsoleColor.Green,
                                            $"Successfully connected to [{sharePath}] with *FULL* Permissions");
                                    else
                                        WriteToConsole(ConsoleColor.Red,
                                            $"Connection to Share [{sharePath}] failed or Permissions invalid");
                                }
                            }
                        }
                    }
                    else
                    {
                        WriteToConsole(ConsoleColor.Red, "Connection to Remote UNC failed. Reason: " + result);
                    }
                }


            }
            catch (Exception exp)
            {
                WriteToConsole(ConsoleColor.Red, "An error occured. Details: " + exp.Message);
            }
                
        }
    }
}
