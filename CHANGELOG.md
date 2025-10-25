# Changelog

Just keeping track of what changes between versions!

## [1.0.0] - 2025-01-26

Initial release!

### Core Features

**Device Profiles**
- Save multiple device configurations (IP, port, name, notes)
- Quick-switch between different ADAM 6060 units
- Auto-test connection when you select a profile
- Everything saves automatically to your AppData folder

**Manual Control**
- Control all 6 digital inputs (DI0-DI5) individually
- Big friendly buttons for SET HIGH and SET LOW
- SET ALL HIGH/LOW when you need to reset everything
- Color-coded indicators (Orange = HIGH, Blue = LOW)
- Controls automatically disable when not connected

**Automated Sequences**
- Build custom sequences of actions (HIGH, LOW, delay)
- Loop them 1-999 times or run forever
- Play/Stop controls
- Save your sequences and load them later
- Reorder actions by moving them up/down

**Status Polling**
- Automatically checks device status every 2 seconds
- See real-time updates as inputs change
- Starts/stops automatically based on connection
- Detects if you lose connection
- Auto-stops polling after 3 failed connection attempts

**Command Log**
- See every command you send
- Timestamps, success/fail indicators
- Keeps the last 100 entries
- Clear it whenever you want

### Technical Details
- Built with .NET 9.0 and WPF for Windows
- Uses MVVM pattern (CommunityToolkit.Mvvm)
- Talks to ADAM devices via UDP
- Saves everything as JSON locally
- Includes automated builds for x64, x86, and ARM64
- Thread-safe state tracking (no more race conditions!)
- Proper resource cleanup when UDP connections timeout
- Robust error handling so the app doesn't crash on network issues

[1.0.0]: https://github.com/dmchristensen/AdamTriggerSimulator/releases/tag/v1.0.0
