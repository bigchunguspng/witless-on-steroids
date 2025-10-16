using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using NS = Newtonsoft.Json;
using ST = System.Text.Json;
using ST_S = System.Text.Json.Serialization;

namespace PF_Tools.Backrooms.Helpers;

public static class JsonIO
{
    // NEWTONSOFT

    private static readonly NS.JsonSerializer
        SerializerDefault = new()
        {
            DefaultValueHandling = NS.DefaultValueHandling.Ignore,
        },
        SerializerIndented = new()
        {
            Formatting = NS.Formatting.Indented,
            DefaultValueHandling = NS.DefaultValueHandling.Ignore,
        };

    public static T LoadData<T>(FilePath path) where T : new()
    {
        if (path.File_DoNotExist_Or_Empty) return New<T>(path);

        using var stream = File.OpenText(path);
        using var reader = new NS.JsonTextReader(stream);
        return SerializerDefault.Deserialize<T>(reader)
            ?? throw new IOException($"Coundn't deserialize an object of type {typeof(T)}");
    }

    public static void SaveData<T>(T db, string path, bool indent = false)
    {
        var serializer = indent ? SerializerIndented : SerializerDefault;
        using var stream = File.CreateText(path);
        using var writer = new NS.JsonTextWriter(stream);
        serializer.Serialize(writer, db);
    }

    private static T New<T>(FilePath path) where T : new()
    {
        if (path.IsNested) path.EnsureParentDirectoryExist();

        T result = new();
        SaveData(result, path);
        return result;
    }

    // SYSTEM.TEXT

    private static readonly ST.JsonSerializerOptions
        JsonOptions_Default = new()
        {
            Encoder = NewtonsoftJsonCompatibleEncoder.Encoder,
            DefaultIgnoreCondition = ST_S.JsonIgnoreCondition.WhenWritingDefault,
        },
        JsonOptions_Indented = new()
        {
            WriteIndented = true,
            Encoder = NewtonsoftJsonCompatibleEncoder.Encoder,
            DefaultIgnoreCondition = ST_S.JsonIgnoreCondition.WhenWritingDefault,
        };

    public static async Task<T> LoadDataAsync<T>(FilePath path) where T : new()
    {
        if (path.File_DoNotExist_Or_Empty) return await NewAsync<T>(path);

        await using var stream = File.OpenRead(path);
        return await ST.JsonSerializer.DeserializeAsync<T>(stream, JsonOptions_Default)
            ?? throw new IOException($"Coundn't deserialize an object of type {typeof(T)}");
    }

    public static async Task SaveDataAsync<T>(T db, string path, bool indent = false)
    {
        var options = indent ? JsonOptions_Indented : JsonOptions_Default;
        await using var stream = File.Create(path);
        await ST.JsonSerializer.SerializeAsync(stream, db, options);
    }

    private static async Task<T> NewAsync<T>(FilePath path) where T : new()
    {
        if (path.IsNested) path.EnsureParentDirectoryExist();

        T result = new();
        await SaveDataAsync(result, path);
        return result;
    }
}

/// Dude, EMOJI! <br/> Newtonsoft.Json compatible JavaScript encoder.
/// <br/> Sauce: https://github.com/RamjotSingh/System.Text.Json.Extensions
/// <br/> Found: https://github.com/dotnet/runtime/issues/54193
public class NewtonsoftJsonCompatibleEncoder : JavaScriptEncoder
{
    private static readonly JavaScriptEncoder _defaultEncoder = UnsafeRelaxedJsonEscaping;
    public  static readonly JavaScriptEncoder         Encoder = new NewtonsoftJsonCompatibleEncoder();

    public override int MaxOutputCharactersPerInputCharacter =>
        _defaultEncoder.MaxOutputCharactersPerInputCharacter;

    public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
    {
        var input = new ReadOnlySpan<char>(text, textLength);
        var index = 0;

        while (Rune.DecodeFromUtf16(input.Slice(index), out var result, out var charsConsumed) == OperationStatus.Done)
        {
            if (WillEncode(result.Value)) break;

            index += charsConsumed;
        }

        return index == input.Length ? -1 : index;
    }

    public override unsafe bool TryEncodeUnicodeScalar
        (int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        => _defaultEncoder.TryEncodeUnicodeScalar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);

    // Quote, slash, control chars -> escape. Everything else -> leave as is.
    public override bool WillEncode
        (int unicodeScalar) => unicodeScalar is '"' or '\\' or < 0x20 && _defaultEncoder.WillEncode(unicodeScalar);
}