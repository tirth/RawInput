using Microsoft.Win32;

namespace RawInput;

static internal class RegistryAccess
{
    static internal RegistryKey? GetDeviceKey(string device)
    {
        var split = device[4..].Split('#');

        var classCode = split[0];       // ACPI (Class code)
        var subClassCode = split[1];    // PNP0303 (SubClass code)
        var protocolCode = split[2];    // 3&13c0b0c5&0 (Protocol code)

        return Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Enum\{classCode}\{subClassCode}\{protocolCode}");
    }

    static internal string GetClassType(string classGuid) 
        => Registry.LocalMachine
            .OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Class\{classGuid}")?
            .GetValue("Class") as string ?? string.Empty;
}
