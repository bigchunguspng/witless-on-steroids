namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_FilePath
{
    // todo make sure all clients call mkdir
    public static FilePath MakeUnique
        (this FilePath path)
    {
        while (path.Exists)
        {
            var name = path.AsSpan_WithoutExtension();
            var ext  = path.AsSpan_Extension();
            var sand = Desert.GetSand(length: 2);
            path = new FilePath($"{name}_{sand}{ext}");
        }
        return path;
    }

    public static FilePath Suffix
        (this FilePath path, string suffix, string extension, char separator = '-') =>
        new ($"{path.AsSpan_WithoutExtension()}{separator}{suffix}{extension}");
    
    public static FilePath Suffix
        (this FilePath path, string suffix1, string suffix2, string extension, char separator = '-') =>
        new ($"{path.AsSpan_WithoutExtension()}{separator}{suffix1}{separator}{suffix2}{extension}");

    public static FilePath ChangeEnding
        (this FilePath path, string suffix_and_extension,     char separator = '-') =>
        new ($"{path.AsSpan_WithoutExtension()}{separator}{suffix_and_extension}");
}