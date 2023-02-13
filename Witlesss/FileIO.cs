using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

namespace Witlesss
{
    public class FileIO<T> where T : new()
    {
        private readonly string _path;
        private readonly JsonSerializerSettings _settings;

        public FileIO(string path)
        {
            _path = path;
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented, 
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public T LoadData()
        {
            if (FileEmptyOrNotExist(_path))
            {
                CreateFilePath(_path);
                File.CreateText(_path).Dispose();
                T result = new();
                SaveData(result);
                return result;
            }

            using var stream = File.OpenText(_path);
            using var reader = new JsonTextReader(stream);
            return new JsonSerializer().Deserialize<T>(reader);
        }

        public void SaveData(T db)
        {
            using var writer = File.CreateText(_path);
            writer.Write(SerializeObject(db, _settings));
        }
    }
}