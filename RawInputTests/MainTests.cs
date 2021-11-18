using RawInput;
using Xunit;
using Xunit.Abstractions;

namespace RawInputTests;

public class MainTests
{
    private readonly ITestOutputHelper _output;

    public MainTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestDeviceList()
    {
        var devices = Win32.GetDevices();

        foreach (var (_, device) in devices)
        {
            _output.WriteLine(device.JsonStr());
        }
    }
}
