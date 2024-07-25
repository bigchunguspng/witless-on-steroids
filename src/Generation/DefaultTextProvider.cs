using System;
using System.Collections.Generic;

namespace Witlesss.Generation;

public static class DefaultTextProvider
{
    private static readonly List<string>? _defaultTexts = new FileIO<List<string>>(Paths.File_DefaultTexts).LoadData();

    public static string? GetRandomResponse()
    {
        if (_defaultTexts is null || _defaultTexts.Count == 0) return null;
        var index = Random.Shared.Next(_defaultTexts.Count);
        return _defaultTexts[index];
    }
}