using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Helpers;

namespace PF_Bot.Core.Text;

public static class Migration_JsonToBinary
{
    public static void MigrateAll()
    {
        var dir_json = new FilePath("Json").EnsureDirectoryExist();
        FilePath[] dirs = [Dir_Chat, Dir_Fuse];
        foreach (var dir in dirs)
        {
            Log($"Migrating {dir}");
            foreach (var file in dir.GetFiles("*.json", recursive: true))
            {
                var sw = Stopwatch.StartNew();

                var bakaV1 = JsonIO.LoadData<PF_Tools.Copypaster_Legacy.Pack.GenerationPack>(file);
                var bakaV2 = new GenerationPack();
                bakaV2.FuseMigrate(bakaV1);

                var name = Path.GetFileNameWithoutExtension(file).Replace("pack-", "");
                var newFileName = $"{name}{Ext_Pack}";
                var path = Path.Combine(dir, newFileName);
                GenerationPackIO.Save(bakaV2, path);

                File.Move(file, dir_json.Combine(dir).EnsureDirectoryExist().Combine(Path.GetFileName(file)));

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
                target.PutTransition(fromId, toId, (transition.Chance * 10).RoundInt());
            }
        }

        int GetNewId(int id) => id < 0 ? id : ids[id];
    }
}