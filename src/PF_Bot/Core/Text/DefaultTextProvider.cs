using PF_Tools.Backrooms.Helpers;

namespace PF_Bot.Core.Text;

public static class DefaultTextProvider
{
    private static readonly List<string>? _defaultTexts =
        JsonIO.LoadData<List<string>>(File_DefaultTexts);

    public static string? GetRandomResponse()
    {
        if (_defaultTexts is null || _defaultTexts.Count == 0) return null;
        var index = Random.Shared.Next(_defaultTexts.Count);
        return _defaultTexts[index];
    }
}