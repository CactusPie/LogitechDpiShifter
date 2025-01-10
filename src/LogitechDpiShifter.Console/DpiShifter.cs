using System.Diagnostics;
using LogitechDpiShifter.Console.ProcessManagement;
using LogitechDpiShifter.Console.ProcessManagement.Data;
using static LogitechDpiShifter.Console.ProcessManagement.ProcessFunctions;

namespace LogitechDpiShifter.Console;

public sealed class DpiShifter
{
    private Process? _targetProcess;

    private IntPtr _processHandle;

    private byte[] _enableDpiShiftHookCode = null!;

    private byte[] _disableDpiShiftHookCode = null!;

    public ulong EnableDpiShiftOriginalFunctionAddress { get; private set; }

    public ulong DisableDpiShiftOriginalFunctionAddress { get; private set; }

    public IntPtr EnableDpiShiftCodeAddress { get; private set; }

    public IntPtr DisableDpiShiftCodeAddress { get; private set; }

    public void Initialize(Process targetProcess)
    {
        ArgumentNullException.ThrowIfNull(targetProcess);
        _targetProcess = targetProcess;
        _processHandle = GetProcessHandle();

        EnableDpiShiftCodeAddress = InjectEnableDpiShiftCode();
        DisableDpiShiftCodeAddress = InjectDisableDpiShiftCode();
    }

    public void Cleanup()
    {
        VirtualFreeEx(_processHandle, EnableDpiShiftCodeAddress, (uint)_enableDpiShiftHookCode.Length, FreeType.Release);
        VirtualFreeEx(_processHandle, DisableDpiShiftCodeAddress, (uint)_disableDpiShiftHookCode.Length, FreeType.Release);
    }

    public void EnableDpiShift()
    {
        if (_targetProcess == null)
        {
            throw new InvalidOperationException($"You must call {nameof(Initialize)} first");
        }

        IntPtr threadHandle = CreateRemoteThread(
            _processHandle,
            IntPtr.Zero,
            0,
            EnableDpiShiftCodeAddress,
            IntPtr.Zero,
            0,
            IntPtr.Zero);

        WaitForSingleObject(threadHandle, uint.MaxValue);
        CloseHandle(threadHandle);
    }

    public void DisableDpiShift()
    {
        if (_targetProcess == null)
        {
            throw new InvalidOperationException($"You must call {nameof(Initialize)} first");
        }

        IntPtr threadHandle = CreateRemoteThread(
            _processHandle,
            IntPtr.Zero,
            0,
            DisableDpiShiftCodeAddress,
            IntPtr.Zero,
            0,
            IntPtr.Zero);

        WaitForSingleObject(threadHandle, uint.MaxValue);
        CloseHandle(threadHandle);
    }

    private IntPtr GetProcessHandle()
    {
        return OpenProcess(
            ProcessAccessFlags.CreateThread | ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VMOperation |
            ProcessAccessFlags.VMWrite | ProcessAccessFlags.VMRead,
            false,
            _targetProcess!.Id
        );
    }

    private IntPtr InjectEnableDpiShiftCode()
    {
        EnableDpiShiftOriginalFunctionAddress = AobScanner.AobScan(
            _targetProcess!.MainModule!,
            _processHandle,
            "48 89 4C 24 08 48 81 EC 38 02 00 00 48 C7 84 24 38");

        byte[] callBytes = BitConverter.GetBytes(EnableDpiShiftOriginalFunctionAddress);

        _enableDpiShiftHookCode =
        [
            0x53, // push rbx
            0x48, 0xBB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx, 01
            0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, // call the address below
            callBytes[0], callBytes[1], callBytes[2], callBytes[3], callBytes[4], callBytes[5], callBytes[6], callBytes[7],
            0x5B, // pop rbx
            0xC3, // ret
        ];

        IntPtr allocatedAddress = VirtualAllocEx(
            _processHandle,
            IntPtr.Zero,
            (uint)_enableDpiShiftHookCode.Length,
            AllocType.Commit | AllocType.Reserve,
            ThreadPermission.ExecuteReadWrite
        );

        WriteProcessMemory(
            _processHandle,
            allocatedAddress,
            _enableDpiShiftHookCode,
            (uint)_enableDpiShiftHookCode.Length,
            out _
        );

        return allocatedAddress;
    }

    private IntPtr InjectDisableDpiShiftCode()
    {
        DisableDpiShiftOriginalFunctionAddress = AobScanner.AobScan(
            _targetProcess!.MainModule!,
            _processHandle,
            "48 89 4C 24 08 48 83 EC 58 48 C7 44 24 30 FE FF FF FF 48 8D 4C 24 38 ?? ?? ?? ?? ?? ?? " +
                "48 89 44 24 20 48 8B 44 24 20 48 89 44 24 28 4C 8B 44 24 28 BA 05");

        byte[] callBytes = BitConverter.GetBytes(DisableDpiShiftOriginalFunctionAddress);

        _disableDpiShiftHookCode =
        [
            0x53, // push rbx
            0x48, 0xBB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx, 01
            0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, // call the address below
            callBytes[0], callBytes[1], callBytes[2], callBytes[3], callBytes[4], callBytes[5], callBytes[6], callBytes[7],
            0x5B, // pop rbx
            0xC3, // ret
        ];

        IntPtr allocatedAddress = VirtualAllocEx(
            _processHandle,
            IntPtr.Zero,
            (uint)_disableDpiShiftHookCode.Length,
            AllocType.Commit | AllocType.Reserve,
            ThreadPermission.ExecuteReadWrite
        );

        WriteProcessMemory(
            _processHandle,
            allocatedAddress,
            _disableDpiShiftHookCode,
            (uint)_disableDpiShiftHookCode.Length,
            out _
        );

        return allocatedAddress;
    }
}