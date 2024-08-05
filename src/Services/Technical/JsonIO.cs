using System.IO;
using Newtonsoft.Json;
using Witlesss.Generation.Pack;

namespace Witlesss.Services.Technical
{
    public static class JsonIO
    {
        private static readonly JsonSerializer SerializerDefault = new()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = { new TransitionTableConverter() }
        };

        private static readonly JsonSerializer SerializerIndented = new()
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = { new TransitionTableConverter() }
        };

        public static T LoadData<T>(string path) where T : new()
        {
            if (FileEmptyOrNotExist(path)) return NewT<T>(path);

            var serializer = SerializerDefault;
            using var stream = File.OpenText(path);
            using var reader = new JsonTextReader(stream);
            return serializer.Deserialize<T>(reader);
        }

        public static void SaveData<T>(T db, string path, bool indent = false)
        {
            var serializer = indent ? SerializerIndented : SerializerDefault;
            using var stream = File.CreateText(path);
            using var writer = new JsonTextWriter(stream);
            serializer.Serialize(writer, db);
        }

        private static T NewT<T>(string path) where T : new()
        {
            if (PathIsNested(path)) CreateFilePath(path);

            T result = new();
            SaveData(result, path);
            return result;
        }

        private static bool PathIsNested(string path) => path.Contains(Path.PathSeparator);
    }
}