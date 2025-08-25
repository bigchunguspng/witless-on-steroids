using Newtonsoft.Json;
using PF_Bot.Generation.Pack;

namespace PF_Bot.Services.Technical
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
            if (path.FileIsEmptyOrNotExist()) return NewT<T>(path);

            var serializer = SerializerDefault;
            using var stream = File.OpenText(path);
            using var reader = new JsonTextReader(stream);
            return serializer.Deserialize<T>(reader)
                ?? throw new IOException($"Coundn't deserialize an object of type {typeof(T)}");
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
            if (path.IsNestedPath()) path.CreateFilePath();

            T result = new();
            SaveData(result, path);
            return result;
        }
    }
}