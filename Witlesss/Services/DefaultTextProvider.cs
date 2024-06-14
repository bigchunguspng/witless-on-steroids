using System;
using System.Collections.Generic;

namespace Witlesss.Services;

public static class DefaultTextProvider
{
    private static readonly List<string>? _defaultTexts = new FileIO<List<string>>("default.json").LoadData();

    public static string? GetRandomResponse()
    {
        if (_defaultTexts is null || _defaultTexts.Count == 0) return null;
        var index = Random.Shared.Next(_defaultTexts.Count);
        return _defaultTexts[index];
    }
}