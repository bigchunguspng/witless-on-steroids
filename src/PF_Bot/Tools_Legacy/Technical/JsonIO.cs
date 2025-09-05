using Newtonsoft.Json;
using PF_Tools.Copypaster_Legacy.Pack;

namespace PF_Bot.Tools_Legacy.Technical
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

        public static T LoadData<T>(FilePath path) where T : new()
        {
            if (path.File_DoNotExist_Or_Empty) return NewT<T>(path);

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

        private static T NewT<T>(FilePath path) where T : new()
        {
            if (path.IsNested) path.EnsureParentDirectoryExist();

            T result = new();
            SaveData(result, path);
            return result;
        }
    }
}