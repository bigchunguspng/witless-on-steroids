using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Helpers;

namespace PF_Bot.Generation;

public static class GenerationPackIO
{
    public static GenerationPack Load(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);
        return BinarySerialization.Deserialize(reader);
    }

    public static void Save(GenerationPack pack, string path)
    {
        var temp = path.Replace(Dir_Chat, Dir_Temp);
        Directory.CreateDirectory(Dir_Temp);

        SaveAs(pack, temp);
        File.Move(temp, path, overwrite: true);
    }

    public static void SaveAs(GenerationPack pack, string path)
    {
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        using var writer = new BinaryWriter(fs);
        BinarySerialization.Serialize(writer, pack);
    }
}