using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace Keylogger.Tools.DetectionDemo
{
    /// <summary>
    /// Educational tool demonstrating techniques to detect keyloggers
    /// For security research and educational purposes only
    /// </summary>
    class KeyloggerDetector
    {
        // Import required Windows API functions
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Hook constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_KEYBOARD = 2;

        // Delegate for the hook callback
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        static void Main(string[] args)
        {
            Console.WriteLine("====================================");
            Console.WriteLine("Keylogger Detection Demo - Blue Team");
            Console.WriteLine("====================================");
            Console.WriteLine();

            Console.WriteLine("This tool demonstrates techniques used to detect keyloggers.");
            Console.WriteLine("Press any key to begin the detection scan...");
            Console.ReadKey(true);
            Console.WriteLine();
            
            Console.WriteLine("Running detection techniques...");
            Console.WriteLine();

            // Run detection methods
            DetectKeyboardHooks();
            DetectSuspiciousProcesses();
            DetectStartupItems();
            DetectFileSystemActivity();
            
            Console.WriteLine("\nScan complete! Press any key to exit...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Detect global keyboard hooks which might indicate a keylogger
        /// </summary>
        private static void DetectKeyboardHooks()
        {
            Console.WriteLine("[1] Checking for keyboard hooks...");
            
            // In a real implementation, you would check for installed hooks
            // This is a simplified educational demonstration
            
            List<string> processesWithKeyboardAccess = new List<string>();
            
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.ProcessName != "explorer" && process.ProcessName != "chrome" &&
                            process.ProcessName != "firefox" && process.ProcessName != "msedge" &&
                            process.ProcessName != "notepad" && process.ProcessName != "devenv" &&
                            process.ProcessName != "System" && !process.ProcessName.StartsWith("Microsoft"))
                        {
                            // Check loaded modules for keyboard-related DLLs
                            // This is a simplified check for educational purposes
                            bool hasPotentialHook = false;
                            try
                            {
                                foreach (ProcessModule module in process.Modules)
                                {
                                    if (module.ModuleName.ToLower().Contains("hook") ||
                                        module.ModuleName.ToLower().Contains("input") ||
                                        module.ModuleName.ToLower().Contains("keyboard"))
                                    {
                                        hasPotentialHook = true;
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                // Can't access modules, potentially suspicious
                            }
                            
                            if (hasPotentialHook)
                            {
                                processesWithKeyboardAccess.Add(process.ProcessName);
                            }
                        }
                    }
                    catch
                    {
                        // Skip process if access is denied
                    }
                }
                
                if (processesWithKeyboardAccess.Count > 0)
                {
                    Console.WriteLine($"  [WARNING] Found {processesWithKeyboardAccess.Count} processes with potential keyboard hooks:");
                    foreach (var proc in processesWithKeyboardAccess)
                    {
                        Console.WriteLine($"    - {proc}");
                    }
                }
                else
                {
                    Console.WriteLine("  [OK] No suspicious keyboard hooks detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERROR] Failed to check keyboard hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Detect processes that might be keyloggers based on names and behavior
        /// </summary>
        private static void DetectSuspiciousProcesses()
        {
            Console.WriteLine("\n[2] Checking for suspicious processes...");
            
            List<string> suspiciousProcessNames = new List<string>
            {
                "keylog", "hooker", "keystroke", "monitor", "capture", "spy",
                "track", "logger", "record", "hook", "stealth"
            };
            
            List<string> detectedProcesses = new List<string>();
            
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        // Check for suspicious process names
                        string procNameLower = process.ProcessName.ToLower();
                        if (suspiciousProcessNames.Any(s => procNameLower.Contains(s)))
                        {
                            detectedProcesses.Add(process.ProcessName);
                            continue;
                        }
                        
                        // Check for high CPU usage in background processes
                        // This is simplified for educational purposes
                        if (process.TotalProcessorTime.TotalSeconds > 30 && 
                            process.MainWindowHandle == IntPtr.Zero &&
                            !IsSystemProcess(process.ProcessName))
                        {
                            detectedProcesses.Add($"{process.ProcessName} (High CPU background process)");
                        }
                    }
                    catch
                    {
                        // Skip process if access is denied
                    }
                }
                
                if (detectedProcesses.Count > 0)
                {
                    Console.WriteLine($"  [WARNING] Found {detectedProcesses.Count} suspicious processes:");
                    foreach (var proc in detectedProcesses)
                    {
                        Console.WriteLine($"    - {proc}");
                    }
                }
                else
                {
                    Console.WriteLine("  [OK] No suspicious processes detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERROR] Failed to check processes: {ex.Message}");
            }
        }

        /// <summary>
        /// Check for suspicious startup entries
        /// </summary>
        private static void DetectStartupItems()
        {
            Console.WriteLine("\n[3] Checking startup items for persistence mechanisms...");
            
            List<string> suspiciousStartupItems = new List<string>();
            
            try
            {
                // Check startup folder
                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                var startupFiles = Directory.GetFiles(startupPath);
                
                foreach (var file in startupFiles)
                {
                    try
                    {
                        FileInfo info = new FileInfo(file);
                        
                        // Check for hidden files
                        if ((info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                        {
                            suspiciousStartupItems.Add($"{info.Name} (Hidden file in Startup folder)");
                            continue;
                        }
                        
                        // Check file names for suspicious terms
                        string lowerName = info.Name.ToLower();
                        if (lowerName.Contains("log") || lowerName.Contains("key") || 
                            lowerName.Contains("hook") || lowerName.Contains("spy") ||
                            lowerName.Contains("monitor") || lowerName.Contains("track"))
                        {
                            suspiciousStartupItems.Add($"{info.Name} (Suspicious name in Startup folder)");
                        }
                    }
                    catch
                    {
                        // Skip file if can't access
                    }
                }
                
                // Check registry startup locations
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            try
                            {
                                string value = key.GetValue(valueName).ToString().ToLower();
                                string lowerValueName = valueName.ToLower();
                                
                                if (lowerValueName.Contains("log") || lowerValueName.Contains("key") || 
                                    lowerValueName.Contains("hook") || lowerValueName.Contains("spy") ||
                                    lowerValueName.Contains("monitor") || lowerValueName.Contains("track") ||
                                    lowerValueName.Contains("security") || lowerValueName.Contains("research"))
                                {
                                    suspiciousStartupItems.Add($"{valueName} (Suspicious registry startup item)");
                                }
                            }
                            catch
                            {
                                // Skip value if can't access
                            }
                        }
                    }
                }
                
                if (suspiciousStartupItems.Count > 0)
                {
                    Console.WriteLine($"  [WARNING] Found {suspiciousStartupItems.Count} suspicious startup items:");
                    foreach (var item in suspiciousStartupItems)
                    {
                        Console.WriteLine($"    - {item}");
                    }
                }
                else
                {
                    Console.WriteLine("  [OK] No suspicious startup items detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERROR] Failed to check startup items: {ex.Message}");
            }
        }

        /// <summary>
        /// Monitor file system activity for keylogger-like behavior
        /// </summary>
        private static void DetectFileSystemActivity()
        {
            Console.WriteLine("\n[4] Monitoring file system for keylogger activity...");
            Console.WriteLine("  Watching for 5 seconds. Please type something while this runs...");
            
            List<string> suspiciousFiles = new List<string>();
            
            try
            {
                // Setup file system watchers for common locations
                FileSystemWatcher docWatcher = new FileSystemWatcher(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                
                FileSystemWatcher tempWatcher = new FileSystemWatcher(
                    Path.GetTempPath());
                
                // Watch for files being written during keyboard input
                docWatcher.Created += (sender, e) => OnFileActivity(e.FullPath, suspiciousFiles);
                docWatcher.Changed += (sender, e) => OnFileActivity(e.FullPath, suspiciousFiles);
                
                tempWatcher.Created += (sender, e) => OnFileActivity(e.FullPath, suspiciousFiles);
                tempWatcher.Changed += (sender, e) => OnFileActivity(e.FullPath, suspiciousFiles);
                
                // Enable watching
                docWatcher.EnableRaisingEvents = true;
                tempWatcher.EnableRaisingEvents = true;
                
                // Wait for a few seconds
                Thread.Sleep(5000);
                
                // Disable watching
                docWatcher.EnableRaisingEvents = false;
                tempWatcher.EnableRaisingEvents = false;
                
                if (suspiciousFiles.Count > 0)
                {
                    Console.WriteLine($"  [WARNING] Detected {suspiciousFiles.Count} files with suspicious activity:");
                    foreach (var file in suspiciousFiles)
                    {
                        Console.WriteLine($"    - {file}");
                    }
                }
                else
                {
                    Console.WriteLine("  [OK] No suspicious file system activity detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERROR] Failed to monitor file system: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle file activity events
        /// </summary>
        private static void OnFileActivity(string path, List<string> suspiciousFiles)
        {
            try
            {
                if (suspiciousFiles.Contains(path)) return;
                
                // Check for suspicious file names or extensions
                string lowerPath = path.ToLower();
                if (lowerPath.Contains("log") || lowerPath.Contains("key") ||
                    lowerPath.Contains("hook") || lowerPath.EndsWith(".dat") ||
                    lowerPath.EndsWith(".bin") || lowerPath.EndsWith(".enc"))
                {
                    suspiciousFiles.Add(path);
                    return;
                }
                
                // Attempt to check content
                try
                {
                    byte[] content = File.ReadAllBytes(path);
                    if (content.Length > 0 && IsLikelyEncrypted(content))
                    {
                        suspiciousFiles.Add($"{path} (Possible encrypted content)");
                    }
                }
                catch
                {
                    // Skip if can't read file
                }
            }
            catch
            {
                // Skip if any errors
            }
        }

        /// <summary>
        /// Check if the byte array appears to be encrypted/encoded content
        /// </summary>
        private static bool IsLikelyEncrypted(byte[] data)
        {
            // Simple entropy-based check for educational purposes
            // High entropy often indicates encryption or compression
            if (data.Length < 20) return false;
            
            Dictionary<byte, int> frequencies = new Dictionary<byte, int>();
            foreach (byte b in data)
            {
                if (!frequencies.ContainsKey(b))
                    frequencies[b] = 0;
                frequencies[b]++;
            }
            
            // Calculate entropy
            double entropy = 0;
            foreach (var kvp in frequencies)
            {
                double probability = (double)kvp.Value / data.Length;
                entropy -= probability * Math.Log(probability, 256);
            }
            
            // High entropy (close to 1.0) suggests encryption/compression
            return entropy > 0.9;
        }

        /// <summary>
        /// Check if a process is a system process
        /// </summary>
        private static bool IsSystemProcess(string processName)
        {
            string[] systemProcesses = 
            {
                "system", "svchost", "services", "smss", "csrss", "wininit",
                "winlogon", "lsass", "spoolsv", "dwm", "taskhost", "explorer"
            };
            
            return systemProcesses.Contains(processName.ToLower());
        }
    }
}