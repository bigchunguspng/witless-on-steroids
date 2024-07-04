using Newtonsoft.Json;
using Witlesss.Generation.Pack;

namespace Witlesss.Services.Technical
{
    public class FileIO<T>(string path) : FileIO_Static where T : new()
    {
        public string Path { get; } = path;

        public T LoadData()
        {
            if (FileEmptyOrNotExist(Path)) return NewT();

            using var stream = File.OpenText(Path);
            using var reader = new JsonTextReader(stream);
            return Serializer.Deserialize<T>(reader);
        }

        public void SaveData(T db)
        {
            using var stream = File.CreateText(Path);
            using var writer = new JsonTextWriter(stream);
            Serializer.Serialize(writer, db);
        }

        private T NewT()
        {
            if (PathIsNested) CreateFilePath(Path);

            T result = new();
            SaveData(result);
            return result;
        }

        private bool PathIsNested => Path.Contains('\\') || Path.Contains('/');
    }

    public class FileIO_Static
    {
        protected static readonly JsonSerializer Serializer = new()
        {
            //Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = { new TransitionTableConverter() }
        };
    }
}