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

    public static void Save(GenerationPack pack, string path, string temp)
    {
        Save(pack, temp);
        path.CreateFilePath();
        File.Move(temp, path, overwrite: true);
    }

    public static void Save(GenerationPack pack, string path)
    {
        path.CreateFilePath();
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        using var writer = new BinaryWriter(fs);
        BinarySerialization.Serialize(writer, pack);
    }
}