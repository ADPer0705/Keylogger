# C# Keylogger Research Platform

[![Educational Use Only](https://img.shields.io/badge/Purpose-Educational%20Only-blue.svg)](LICENSE)
[![C#](https://img.shields.io/badge/Language-C%23-green.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-purple.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)

## 🔬 Research Purpose

This project implements a comprehensive keylogging system in C# for **educational and security research purposes only**. It serves as a platform for studying endpoint monitoring techniques, user input tracking, and defensive countermeasures.

## 🛠️ Technical Architecture

### Core Components

#### Input Monitoring Engine
- Windows API hooks via P/Invoke (`user32.dll`) for keyboard state monitoring
- Virtual key code interpretation and translation layer
- Active window context tracking through Win32 API integration

#### Data Processing Pipeline  
- AES-256 encryption for data at rest
- Custom key event formatting and serialization
- Timestamped window context correlation

#### Anti-Detection Subsystem
- Process enumeration and analysis for VM/analysis tool detection
- Console window management via Win32 API
- Defensive fingerprinting techniques

#### Persistence Module
- Windows Script Host COM automation for shortcut creation
- Startup folder integration
- Clean uninstallation capabilities

## 🧪 Development Environment

- **IDE**: Visual Studio 2022
- **Framework**: .NET Framework 4.8
- **Target Platform**: Windows 10/11
- **Testing Environment**: Isolated virtual machines

## 📋 API Reference

### Native Windows API Integration
```csharp
[DllImport("user32.dll")]
private static extern int GetAsyncKeyState(int vKey);

[DllImport("user32.dll")]
private static extern IntPtr GetForegroundWindow();

[DllImport("user32.dll")]
private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

[DllImport("user32.dll")]
private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
```

### Key Component Methods
```csharp
// Hook keyboard and begin monitoring
static void CaptureKeys();

// Process and store encrypted keystroke data
static void LogKeypress(string key);

// Implement stealth techniques
static void HideConsoleWindow();

// Analyze runtime environment for security tools
static bool DetectSecurityTools();
```

## 📘 Usage Reference

### Build Configuration
```
- Debug: Includes console output for development feedback
- Release: Optimized build with minimal footprint
```

### Command-Line Interface
```
keylogger.exe              // Standard execution
keylogger.exe -decrypt     // Decrypt and display logged data
keylogger.exe -install     // Install persistence mechanism
keylogger.exe -uninstall   // Remove persistence
```

## 📚 Technical Documentation

Additional documentation:

- [Development Roadmap](ROADMAP.md)
- [Technical Implementation Details](TECHNICAL_DETAILS.md)
- [Contribution Guidelines](CONTRIBUTING.md)
- [Security Considerations](SECURITY.md)

## ⚠️ Research Ethics Statement

This software is designed **exclusively for security education and research purposes**. The codebase intentionally includes comments and documentation to facilitate learning about operating system internals, API usage patterns, and security concepts.

The developers assume no liability for misuse. Always adhere to applicable laws and ethical standards when conducting security research.