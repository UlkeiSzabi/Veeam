
# Veeam C# Developer in QA Test Task

## Description

A small .NET 8.0 console application developed in C# as part of the C# Developer in QA
testing process.

The application's purpose is to monitor a user specified process, checking it in various time intervals and terminating it if its lifetime exceeds a user specified limit.

## Documentation

### MonitorService

Dependencies:
- Microsoft.Extensions.Hosting 8.0.0

The MonitorService class extends the BackgroundService class and implements it's ExecuteAsync Task. Through this task the given process is monitored periodically and is terminated if its lifetime exceeds the maximum.

### Main

The entry point, Main Task is responsible for:
- Initial argument parsing and verification
- Building and configuring the Host application with the required logging feature and background service configured.
- Listening for the stopping token while the host is running
  - Q for integrated terminals
  - ENTER for redirected consoles (Bash, Cygwin etc.)

## Tests

Dependencies:
- NUnit 4.1.0
- NUnit3TestAdapter 4.6.0

The tests can be found under VeeamTests/UnitTests.cs
The test base checks for:
- Fault injection (invalid parameters)
- Feature testing
- Logging

## Usage/Examples

The application expects 3 arguments:
- process name : non-null string
  - the process to be monitored
- lifetime : positive integer
  - the maximum lifetime the specified process is allowed in minutes
- check interval : positive integer
  - the checking interval

```Bash
./VeeamTestTask.exe Notepad 5 1
```
> The above command will check for the runtime of the Notepad process every other minute and will terminate it if its runtime exceeds the given 5 minutes.
