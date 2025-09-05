using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Helpers;

namespace PF_Bot.Core.Generation;

public static class GenerationPackIO
{
    public static GenerationPack Load(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);
        return BinarySerialization.Deserialize(reader);
    }

    /// Saves pack to a temp~ file first, then copies it to given path.
    /// Make sure directory exist!
    public static void Save_WithTemp(GenerationPack pack, FilePath path)
    {
        var temp = $"{path}~";
        Save(pack, temp);
        File.Move(temp, path, overwrite: true);
    }

    /// Saves pack directly to a file.
    /// Make sure directory exist!
    public static void Save(GenerationPack pack, FilePath path)
    {
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        using var writer = new BinaryWriter(fs);
        BinarySerialization.Serialize(writer, pack);
    }
}