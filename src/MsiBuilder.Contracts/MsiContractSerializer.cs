using System.Text.Json;
using System.Text.Json.Serialization;

namespace MsiBuilder.Contracts;

/// <summary>
/// Shared JSON (de)serialization for the build contracts. Centralizing the options here guarantees the
/// UI (net10) and the worker (net472) read and write byte-compatible JSON for both the worker handshake
/// and the saved build profiles.
/// </summary>
public static class MsiContractSerializer
{
    /// <summary>The single set of serializer options used on both sides.</summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    /// <summary>Serializes <paramref name="value"/> using <see cref="Options"/>.</summary>
    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, Options);

    /// <summary>Deserializes JSON into <typeparamref name="T"/> using <see cref="Options"/>.</summary>
    public static T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, Options);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
