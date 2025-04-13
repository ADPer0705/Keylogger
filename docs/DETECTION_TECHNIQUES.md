# Keylogger Detection Techniques

This document outlines various techniques for detecting keyloggers, providing educational value for security researchers and defenders.

## Behavioral Detection Methods

### 1. API Call Monitoring

Keyloggers typically use specific Windows API calls to intercept keyboard input:

| API Call | Suspicious Pattern | Detection Approach |
|---------|-------------------|-------------------|
| `GetAsyncKeyState` | High-frequency polling in a background process | Monitor processes making repeated calls to this API |
| `SetWindowsHookEx` with `WH_KEYBOARD` or `WH_KEYBOARD_LL` | Installation of global keyboard hooks | Look for processes installing these hook types |
| `GetForegroundWindow` + `GetWindowText` | Frequent window context monitoring | Identify processes tracking active windows while logging keyboard input |
| `RegisterRawInputDevices` | Registering for raw input from keyboard devices | Monitor for unexpected processes registering for raw input |

### 2. Process Analysis

| Indicator | Description | Detection Method |
|----------|-------------|-----------------|
| Hidden windows | Processes that capture input but have no visible UI | Enumerate windows with `EnumWindows` and check visibility attributes |
| Background CPU usage | Keyloggers often run in polling loops | Monitor for background processes with consistent low CPU usage patterns |
| Process names | Suspicious or obfuscated process names | Check for processes with randomly generated names or names attempting to mimic system processes |
| Module injection | Keyloggers may inject into legitimate processes | Look for unexpected modules loaded in processes that handle user input |

### 3. File System Monitoring

| Indicator | Description | Detection Method |
|----------|-------------|-----------------|
| Log file creation | Files being created/modified after keyboard input | Monitor file I/O operations correlating with keyboard activity |
| High entropy files | Encrypted or encoded log files | Scan for files with entropy characteristics of encrypted content |
| Hidden files | Files with hidden attributes | Search for hidden files in common logging locations |
| Suspicious file names | Files with names attempting to appear legitimate | Scan for files with names like "svchost.dat" or other system-like names |

## Persistence Mechanism Detection

### 1. Startup Location Monitoring

| Location | Detection Approach | Educational Value |
|---------|-------------------|-------------------|
| Startup Folder | Monitor for new files in `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup` | Simple persistence mechanism commonly used by malware |
| Run Keys | Monitor registry keys like `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` | Common persistence technique that survives reboots |
| Scheduled Tasks | Check for new scheduled tasks with suspicious actions | More sophisticated persistence that can specify detailed execution conditions |
| Service Creation | Monitor for new services or modifications to existing services | Elevated persistence method that can run with system privileges |

### 2. Advanced Persistence Detection

| Technique | Description | Detection Method |
|----------|-------------|-----------------|
| WMI Event Subscription | Persistent WMI event consumers that execute code | Query for suspicious `__EventFilter`, `__EventConsumer`, and `__FilterToConsumerBinding` instances |
| DLL Search Order Hijacking | Placing malicious DLLs in locations where they'll be loaded by legitimate applications | Monitor for unexpected DLLs in application directories |
| Boot/Logon Autostart | Autostart executables from startup locations | Monitor Registry Run keys and common startup locations |
| Accessibility Features | Replacing accessibility executables with malicious versions | Monitor integrity of accessibility tools like osk.exe, magnify.exe, etc. |

## Memory Analysis Techniques

| Technique | Description | Educational Value |
|----------|-------------|-------------------|
| API Hook Detection | Identify hooks installed in running processes | Detects sophisticated keyloggers that hook input functions |
| Memory Pattern Scanning | Search for signatures of known keylogger code | Helps identify known keyloggers even if files are hidden |
| Callback Function Identification | Locate suspicious keyboard-related callback functions | Identifies keyloggers using legitimate input APIs |
| Memory-resident Module Detection | Find modules loaded in memory but not on disk | Detects fileless keyloggers |

## Network-Based Detection

| Indicator | Description | Detection Method |
|----------|-------------|-----------------|
| Data Exfiltration | Keystroke data being sent over the network | Monitor for patterns of regular, small network packets |
| Command & Control | Communication with external servers | Look for unexpected connections from processes monitoring keyboard input |
| DNS Tunneling | Using DNS queries to exfiltrate data | Monitor for high volumes of DNS queries or unusually large DNS packets |
| Encrypted Connections | Secure connections initiated by untrusted processes | Identify unexpected SSL/TLS connections from unfamiliar processes |

## Defensive Recommendations

1. **Application Whitelisting**: Only allow known, trusted applications to run
2. **Input Method Protection**: Use secure input methods that bypass standard keyboard APIs
3. **Behavior-Based Security Solutions**: Deploy security solutions that can detect suspicious process behavior
4. **Regular Security Scans**: Perform periodic scans focused on detecting monitoring tools
5. **Hardware Security Keys**: Use hardware security keys for sensitive credential entry
6. **Virtual Keyboards**: For sensitive data entry, consider on-screen keyboards
7. **Multi-factor Authentication**: Implement MFA to mitigate the risk of stolen credentials

## Educational Lab Exercises

1. **API Hooking Detection Lab**: Create a tool that detects processes hooking keyboard-related APIs
2. **Memory Forensics Challenge**: Analyze memory dumps to identify hidden keyloggers
3. **Behavioral Analysis Exercise**: Monitor system API calls to identify suspicious patterns
4. **Persistence Hunting**: Develop scripts to identify common keylogger persistence mechanisms

## Real-World Detection Scenario

Here's a typical detection workflow that security professionals might implement:

1. **Initial Indicator**: Unusual process making frequent calls to `GetAsyncKeyState`
2. **Process Investigation**: Examination reveals no visible UI but active keyboard monitoring
3. **Persistence Check**: Discovery of startup folder entry or registry run key
4. **File Analysis**: Identification of encrypted log files
5. **Network Analysis**: Detection of periodic data transfers correlating with typing activity
6. **Remediation**: Process termination, persistence removal, and root cause analysis

## Conclusion

Effective keylogger detection requires a multi-layered approach combining API monitoring, process behavior analysis, file system monitoring, and network traffic inspection. Understanding these techniques provides valuable insights for both security researchers studying malware behaviors and defenders developing protective measures.

Remember that this information is provided for educational purposes only, to help understand both offensive techniques and their defensive countermeasures.