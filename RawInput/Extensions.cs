using System.Text.Json;

namespace RawInput;

public static class Extensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public static string JsonStr<T>(this T obj, JsonSerializerOptions? opts = null)
        => JsonSerializer.Serialize(obj, opts ?? _jsonSerializerOptions);
}
