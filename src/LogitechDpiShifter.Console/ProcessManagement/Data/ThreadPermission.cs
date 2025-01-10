namespace LogitechDpiShifter.Console.ProcessManagement.Data;

[Flags]
public enum ThreadPermission
{
    Execute = 0x10,
    ExecuteRead = 0x20,
    ExecuteReadWrite = 0x40,
}