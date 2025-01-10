using System.Runtime.InteropServices;
using LogitechDpiShifter.Console.ProcessManagement.Data;

namespace LogitechDpiShifter.Console.ProcessManagement;

internal static class ProcessFunctions
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(
        ProcessAccessFlags dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        uint nSize,
        out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern int CloseHandle(IntPtr hProcess);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        [Out] byte[] lpBuffer,
        int dwSize,
        out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        uint dwSize,
        AllocType flAllocationType,
        ThreadPermission flProtect);

    [DllImport("kernel32.dll")]
    public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, FreeType dwFreeType);

    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        IntPtr lpThreadId);

    [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
    public static extern int WaitForSingleObject(IntPtr handle, uint milliseconds);
}