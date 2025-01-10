using WindowsInput.Events;
using WindowsInput.Events.Sources;

namespace LogitechDpiShifter.Console;

public static class InputHandler
{
    public static IDisposable HandleInputEvents(DpiShifter dpiShifter)
    {
        bool dpiShiftEnabled = false;
        IMouseEventSource mouse = WindowsInput.Capture.Global.MouseAsync();

        mouse.MouseEvent += (_, eventArgs) =>
        {
            if (eventArgs.Data.ButtonDown?.Button == ButtonCode.Right)
            {
                if (!dpiShiftEnabled)
                {
                    dpiShifter.EnableDpiShift();
                    dpiShiftEnabled = true;
                }
            }
            else if (eventArgs.Data.ButtonUp?.Button == ButtonCode.Right)
            {
                if (dpiShiftEnabled)
                {
                    dpiShifter.DisableDpiShift();
                    dpiShiftEnabled = false;
                }
            }
        };

        return mouse;
    }
}