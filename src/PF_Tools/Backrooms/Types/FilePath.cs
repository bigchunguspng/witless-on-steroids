namespace PF_Tools.Backrooms.Types;

/// <see cref="String"/> that represents file or directory path.
public readonly struct FilePath(string value)
{
    private readonly string _path
        = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentNullException(nameof(value))
            : value;

    public          string Value      => _path;
    public override string ToString() => _path;

    public static implicit operator string(FilePath path) => path._path;
    public static implicit operator FilePath(string path) => new (path);


    // COMBINE

    /// <inheritdoc cref="Path.Combine(string, string)"/>
    public FilePath Combine(string other)
        =>     Path.Combine(_path, other);
    
    /// <inheritdoc cref="Path.Combine(string, string, string)"/>
    public FilePath Combine(string other1, string other2)
        =>     Path.Combine(_path, other1, other2);

    /// <inheritdoc cref="Path.Combine(string[])"/>
    public FilePath Combine(params string[] others)
        =>     Path.Combine([_path, ..others]);

    /// <inheritdoc cref="Path.ChangeExtension(string?, string?)"/>
    public FilePath ChangeExtension(string extension)
        =>     Path.ChangeExtension(_path, extension);

    // PARTS

    /// <inheritdoc cref="Path.GetFileName(string?)"/>
    public string  FileName
        => Path.GetFileName(_path);

    /// <inheritdoc cref="Path.GetFileNameWithoutExtension(string?)"/>
    public string  FileNameWithoutExtension
        => Path.GetFileNameWithoutExtension(_path);

    /// <inheritdoc cref="Path.GetExtension(string?)"/>
    public string  Extension
        => Path.GetExtension(_path);

    /// <inheritdoc cref="Path.GetDirectoryName(string?)"/>
    public string? DirectoryName
        => Path.GetDirectoryName(_path);

    // SPAN

    public ReadOnlySpan<char> AsSpan_WithoutExtension()
    {
        var path = _path.AsSpan();
        var index_dot = path.LastIndexOf('.');
        return index_dot < 0
            ? path
            : path[..index_dot];
    }

    public ReadOnlySpan<char> AsSpan_Extension()
    {
        var path = _path.AsSpan();
        var index_dot = path.LastIndexOf('.');
        return index_dot < 0
            ? path
            : path[index_dot..];
    }

    // EXISTS

    /// <inheritdoc cref="Path.Exists(string?)"/>
    public bool Exists
        => Path.Exists(_path);

    /// <inheritdoc cref="File.Exists(string?)"/>
    public bool FileExists
        => File.Exists(_path);

    /// <inheritdoc cref="Directory.Exists(string?)"/>
    public bool DirectoryExists
        => Directory.Exists(_path);

    public bool IsNested
        => _path.Contains(Path.   DirectorySeparatorChar)
        || _path.Contains(Path.AltDirectorySeparatorChar);

    public bool File_DoNotExist_Or_Empty
        => FileExists == false || FileSize == 0;

    // SIZE

    public long FileSizeInBytes
        => FileExists ? FileSize : 0;

    private long FileSize => new FileInfo(_path).Length;

    // CREATE DIRECTORY

    public FilePath EnsureParentDirectoryExist()
    {
        var directory = DirectoryName;
        if (directory != null) Directory.CreateDirectory(directory);

        return this;
    }

    public FilePath EnsureDirectoryExist()
    {
        Directory.CreateDirectory(_path);
        return this;
    }

    // GET FILES

    /// Makes sure directory exists.
    /// <inheritdoc cref="DirectoryInfo.GetFiles(string, SearchOption)"/>>
    public FileInfo[] GetFilesInfo
        (string pattern = "*", bool recursive = false)
    {
        Directory.CreateDirectory(_path);
        return  new DirectoryInfo(_path).GetFiles(pattern, GetSearchOption(recursive));
    }

    /// Makes sure directory exists.
    /// <inheritdoc cref="Directory.GetFiles(string, string, SearchOption)"/>
    public string[] GetFiles
        (string pattern = "*", bool recursive = false)
    {
        Directory.CreateDirectory(_path);
        return Directory.GetFiles(_path,          pattern, GetSearchOption(recursive));
    }

    private static SearchOption GetSearchOption
        (bool recursive) => recursive
        ? SearchOption.AllDirectories
        : SearchOption.TopDirectoryOnly;

}