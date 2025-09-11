using PF_Tools.Copypaster;

namespace PF_Bot.Core.Text;

public static class Migration_JsonToBinary
{
    public static void MigrateAll()
    {
        string[] dirs = [Dir_Chat, Dir_Fuse];
        var options = new EnumerationOptions { RecurseSubdirectories = true };
        foreach (var dir in dirs)
        {
            Log($"Migrating {dir}");
            var files = Directory.GetFiles(dir, "*.json", options);
            foreach (var file in files)
            {
                var sw = Stopwatch.StartNew();

                var bakaV1 = JsonIO.LoadData<PF_Tools.Copypaster_Legacy.Pack.GenerationPack>(file);
                var bakaV2 = new GenerationPack();
                bakaV2.FuseMigrate(bakaV1);

                var name = Path.GetFileNameWithoutExtension(file).Replace("pack-", "");
                var newFileName = $"{name}{Ext_Pack}";
                var path = Path.Combine(dir, newFileName);
                GenerationPackIO.Save(bakaV2, path);
                
                sw.Log($"Migrated {file}");
            }
        }
    }

    public static void FuseMigrate(this GenerationPack target, PF_Tools.Copypaster_Legacy.Pack.GenerationPack source)
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