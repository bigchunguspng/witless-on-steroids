using System.IO;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;
using static Witlesss.Extension;

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
                CreatePath(_path);
                File.CreateText(_path).Dispose();
                T result = new T();
                SaveData(result);
                return result;
            }

            using var reader = File.OpenText(_path);
            return DeserializeObject<T>(reader.ReadToEnd());
        }

        public void SaveData(T db)
        {
            using var writer = File.CreateText(_path);
            writer.Write(SerializeObject(db, _settings));
        }
    }
}