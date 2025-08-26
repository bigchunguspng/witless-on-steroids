using PF_Tools.Copypaster;

namespace PF_Bot.Generation;

public static class MigrationExtensions
{
    public static void MigrateAll()
    {
        string[] dirs = [Dir_Chat, Dir_Fuse];
        var options = new EnumerationOptions { RecurseSubdirectories = true };
        foreach (var dir in dirs)
        {
            var files = Directory.GetFiles(dir, "*.json", options);
            foreach (var file in files)
            {
                var bakaV1 = JsonIO.LoadData<PF_Bot.Generation.Pack.GenerationPack>(file);
                var bakaV2 = new GenerationPack();
                bakaV2.FuseMigrate(bakaV1);

                var newFileName = $"{Path.GetFileNameWithoutExtension(file).Replace("pack-", "")}.bin";
                var path = Path.Combine(dir, newFileName);
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                using var writer = new BinaryWriter(fs);
            }
        }
    }
    public static void FuseMigrate(this GenerationPack target, Pack.GenerationPack source)
    {
        // update vocabulary
        var ids = source.Vocabulary.Select(target.TryAddWord_GetWordId).ToList();

        // update transitions
        foreach (var table in source.Transitions)
        {
            var fromId = GetNewId(table.Key);
            foreach (var transition in table.Value.AsIEnumerable())
            {
                var toId = GetNewId(transition.WordID);
                target.PutTransition(fromId, toId, transition.Chance);
            }
        }

        int GetNewId(int id) => id < 0 ? id : ids[id];
    }
}