using System.Text.Json;
using System.Text.Json.Serialization;

namespace PF_Tools.Copypaster.Helpers;

public static class GenerationPackIO
{
    // BINARY

    /// Loads pack into memory.
    /// Or creates a new empty one, if the file doesn't exist.
    public static GenerationPack Load(FilePath path, bool nzb = false)
    {
        if (path.FileExists.Janai()) return new GenerationPack();

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return BinarySerialization.Deserialize(fs, nzb);
    }

    /// Saves pack to a temp~ file first, then copies it to given path.
    /// Make sure directory exist!
    public static void Save_WithTemp(GenerationPack pack, FilePath path, bool nzb = false)
    {
        var temp = $"{path}~";
        Save(pack, temp, nzb);
        File.Move(temp, path, overwrite: true);
    }

    /// Saves pack directly to a file.
    /// Make sure directory exist!
    public static void Save(GenerationPack pack, FilePath path, bool nzb = false)
    {
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        BinarySerialization.Serialize(fs, pack, nzb);
    }

    // JSON

    /// Saves pack to a json file.
    /// Intended to be used for pack inspection.
    public static async Task Save_Json(GenerationPack pack, FilePath path)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, pack, _options);
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new GenerationPackJsonConverter() },
        Encoder = NewtonsoftJsonCompatibleEncoder.Encoder,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };
}