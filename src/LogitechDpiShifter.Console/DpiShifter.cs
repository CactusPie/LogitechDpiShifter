using System.Diagnostics;
using LogitechDpiShifter.Console.ProcessManagement.Data;
using static LogitechDpiShifter.Console.ProcessManagement.ProcessFunctions;

namespace LogitechDpiShifter.Console;

public sealed class DpiShifter
{
    private Process? _targetProcess;

    private IntPtr _processHandle;

    private readonly byte[] _enableDpiShiftCode =
    [
        0x53, // push rbx
        0x48, 0xBB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx, 01
        0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, // call the address below
        0x20, 0x32, 0x59, 0xBE, 0xF7, 0x7F, 0x00, 0x00, // 7FF7BE593220 - the address of the function responsible for enabling the DPI shift
        0x5B, // pop rbx
        0xC3, // ret
    ];

    private readonly byte[] _disableDpiShiftCode =
    [
        0x53, // push rbx
        0x48, 0xBB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx, 01
        0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, // call the address below
        0x70, 0x37, 0x59, 0xBE, 0xF7, 0x7F, 0x00, 0x00, // 7FF7BE593770 - the address of the function responsible for enabling the DPI shift
        0x5B, // pop rbx
        0xC3, // ret
    ];

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
        VirtualFreeEx(_processHandle, EnableDpiShiftCodeAddress, (uint)_enableDpiShiftCode.Length, FreeType.Release);
        VirtualFreeEx(_processHandle, DisableDpiShiftCodeAddress, (uint)_disableDpiShiftCode.Length, FreeType.Release);
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
        IntPtr allocatedAddress = VirtualAllocEx(
            _processHandle,
            IntPtr.Zero,
            (uint)_enableDpiShiftCode.Length,
            AllocType.Commit | AllocType.Reserve,
            ThreadPermission.ExecuteReadWrite
        );

        WriteProcessMemory(
            _processHandle,
            allocatedAddress,
            _enableDpiShiftCode,
            (uint)_enableDpiShiftCode.Length,
            out _
        );

        return allocatedAddress;
    }

    private IntPtr InjectDisableDpiShiftCode()
    {
        IntPtr allocatedAddress = VirtualAllocEx(
            _processHandle,
            IntPtr.Zero,
            (uint)_disableDpiShiftCode.Length,
            AllocType.Commit | AllocType.Reserve,
            ThreadPermission.ExecuteReadWrite
        );

        WriteProcessMemory(
            _processHandle,
            allocatedAddress,
            _disableDpiShiftCode,
            (uint)_disableDpiShiftCode.Length,
            out _
        );

        return allocatedAddress;
    }
}