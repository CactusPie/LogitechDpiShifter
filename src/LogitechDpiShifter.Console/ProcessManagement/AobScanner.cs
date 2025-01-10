using System.Diagnostics;

namespace LogitechDpiShifter.Console.ProcessManagement;

public static class AobScanner
{
    public static ulong AobScan(ProcessModule processModule, IntPtr processHandle, string pattern)
    {
        byte?[] patternBytes = ConvertToByteArray(pattern);

        byte[] localModuleBytes = new byte[processModule.ModuleMemorySize];
        ProcessFunctions.ReadProcessMemory(
            processHandle,
            processModule.BaseAddress,
            localModuleBytes,
            processModule.ModuleMemorySize,
            out IntPtr _);

        for (int localModuleAddress = 0; localModuleAddress < localModuleBytes.Length; localModuleAddress++)
        {
            bool match = true;

            for (var patternIndex = 0; patternIndex < patternBytes.Length; patternIndex++)
            {
                if (localModuleAddress + patternIndex >= localModuleBytes.Length)
                {
                    return 0;
                }

                if (patternBytes[patternIndex] == null)
                {
                    continue;
                }

                if (patternBytes[patternIndex] != localModuleBytes[localModuleAddress + patternIndex])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return (ulong)processModule.BaseAddress + (ulong)localModuleAddress;
            }
        }

        return 0;
    }

    private static byte?[] ConvertToByteArray(string pattern)
    {
        var convertedArray = new List<byte?>(pattern.Length / 3);
        foreach (string patternPart in pattern.Split(' '))
        {
            if (patternPart == "??")
            {
                convertedArray.Add(null);
            }
            else
            {
                convertedArray.Add(Convert.ToByte(patternPart, 16));
            }
        }

        return convertedArray.ToArray();
    }
}