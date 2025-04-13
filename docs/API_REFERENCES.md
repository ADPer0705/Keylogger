# Windows API References

This document provides detailed information about the Windows API functions used in the keylogger research project.

## User32.dll Functions

### GetAsyncKeyState

```csharp
[DllImport("user32.dll")]
private static extern int GetAsyncKeyState(int vKey);
```

**Purpose:** Determines whether a key is up or down at the time the function is called, and whether the key was pressed after a previous call to GetAsyncKeyState.

**Educational Value:** This function is commonly used by keyloggers because it can detect keystrokes system-wide without installing a hook. It's simple to implement but has the drawback of potentially missing keystrokes if not called frequently enough.

**Detection:** Security software often monitors for processes making rapid, repeated calls to this function, especially when combined with file writes.

### GetForegroundWindow

```csharp
[DllImport("user32.dll")]
private static extern IntPtr GetForegroundWindow();
```

**Purpose:** Retrieves a handle to the foreground window (the window with which the user is currently working).

**Educational Value:** Keyloggers use this to provide context for captured keystrokes, understanding which application the user was interacting with when keys were pressed.

**Detection:** Frequent polling of this function from a background process can be a sign of monitoring behavior.

### GetWindowText

```csharp
[DllImport("user32.dll")]
private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
```

**Purpose:** Copies the text of the specified window's title bar into a buffer.

**Educational Value:** Used to obtain the title of the active window, providing context for captured keystrokes.

**Detection:** When combined with GetForegroundWindow and called frequently, this can indicate a monitoring application.

### ShowWindow

```csharp
[DllImport("user32.dll")]
private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
```

**Purpose:** Sets the specified window's show state.

**Educational Value:** Malware commonly uses this to hide console windows or other UI elements that would otherwise reveal its presence to the user.

**Detection:** The use of this API with the SW_HIDE (0) parameter is suspicious for console applications.

### SetWindowsHookEx

```csharp
[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
```

**Purpose:** Installs an application-defined hook procedure into a hook chain.

**Educational Value:** This is a more sophisticated method for intercepting keystrokes than GetAsyncKeyState, allowing for the creation of keyboard hooks that receive all keystroke messages before they reach applications.

**Detection:** Security software specifically looks for WH_KEYBOARD and WH_KEYBOARD_LL hook types.

## Kernel32.dll Functions

### GetModuleHandle

```csharp
[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
private static extern IntPtr GetModuleHandle(string lpModuleName);
```

**Purpose:** Retrieves a module handle for the specified module.

**Educational Value:** Often used in conjunction with SetWindowsHookEx to obtain the handle of the module containing the hook procedure.

**Detection:** Not inherently suspicious, but its use alongside keyboard hooks is noteworthy.

## Registry and File System Operations

While not direct API calls, the following operations are relevant for understanding keylogger behavior:

### Registry Startup Persistence

```csharp
using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
{
    key.SetValue("ApplicationName", executablePath);
}
```

**Purpose:** Creates a registry entry to run the application when the user logs in.

**Educational Value:** Demonstrates a common persistence mechanism used by malware.

**Detection:** Security software monitors changes to autorun registry locations.

### Startup Folder Persistence

```csharp
string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
string shortcutPath = Path.Combine(startupFolderPath, "Application.lnk");
// Create shortcut at shortcutPath
```

**Purpose:** Creates a shortcut in the Windows Startup folder to run on login.

**Educational Value:** Shows another common persistence mechanism that's easy for users to find but still widely used.

**Detection:** File monitoring for new items in the Startup folder.

## Defensive Countermeasures

### API Monitoring

Security products employ various techniques to detect suspicious API usage:

1. **API Hooking:** Security software may hook these same APIs to monitor for malicious use
2. **Frequency Analysis:** Detecting rapid, repeated calls to keyboard-related functions
3. **Context Analysis:** Examining the legitimacy of processes using these APIs

### Real-time Protection

Modern security solutions examine:

1. **Process Behavior:** Is a process collecting keyboard input without visible UI?
2. **File Operations:** Is keystroke data being written to files?
3. **Network Activity:** Is keyboard data being exfiltrated over the network?

## Educational Notes

Understanding these APIs helps security professionals:

1. **Develop Detection Rules:** Create signatures for malicious behavior
2. **Perform Forensic Analysis:** Identify compromised systems
3. **Implement Preventative Measures:** Design systems resistant to keylogging

Remember that all code in this project is provided for educational purposes only, to understand both offensive techniques and defensive countermeasures.