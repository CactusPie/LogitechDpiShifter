using System.Runtime.InteropServices;

namespace LogitechDpiShifter.Console.ProcessManagement;

public static class ConsoleClosedHandler
{
    [DllImport("Kernel32.dll")]
    private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

    // https://learn.microsoft.com/en-us/windows/console/handlerroutine?WT.mc_id=DT-MVP-5003978
    private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);

    public static event EventHandler<EventArgs> OnConsoleClosed;

    static ConsoleClosedHandler()
    {
        SetConsoleCtrlHandler(Handler, true);
    }

    private static bool Handler(CtrlType signal)
    {
        switch (signal)
        {
            case CtrlType.CTRL_BREAK_EVENT:
            case CtrlType.CTRL_C_EVENT:
            case CtrlType.CTRL_LOGOFF_EVENT:
            case CtrlType.CTRL_SHUTDOWN_EVENT:
            case CtrlType.CTRL_CLOSE_EVENT:
                var handler = OnConsoleClosed;
                handler.Invoke(null, EventArgs.Empty);
                Environment.Exit(0);
                return false;

            default:
                return false;
        }
    }

    private enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }
}