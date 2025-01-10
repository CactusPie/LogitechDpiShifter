using System.Diagnostics;
using LogitechDpiShifter.Console;
using LogitechDpiShifter.Console.ProcessManagement;

Process? process = Process.GetProcessesByName("LCore").FirstOrDefault();

if (process == null)
{
    Console.WriteLine("Could not find LCore.exe process. Make sure Logitech Gaming Software is Running");
    return;
}

process.EnableRaisingEvents = true;

var logitechGamingSoftwareClosedEvent = new ManualResetEvent(false);
process.Exited += (_, _) =>
{
    logitechGamingSoftwareClosedEvent.Set();
};

var dpiShifter = new DpiShifter();
dpiShifter.Initialize(process);

Console.WriteLine($"Enable DPI shifter code address: {dpiShifter.EnableDpiShiftCodeAddress:x8}");
Console.WriteLine($"Disable DPI shifter code address: {dpiShifter.EnableDpiShiftCodeAddress:x8}");

ConsoleClosedHandler.OnConsoleClosed += (_, _) =>
{
    dpiShifter.Cleanup();
};

dpiShifter.DisableDpiShift();

using IDisposable inputHandler = InputHandler.HandleInputEvents(dpiShifter);

Console.WriteLine("Successfully attached");
logitechGamingSoftwareClosedEvent.WaitOne();
Console.WriteLine("Logitech Gaming Software Closed. Exiting.");

