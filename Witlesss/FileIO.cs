using System.IO;
using Newtonsoft.Json;
using static Witlesss.Also.Extension;

namespace Witlesss
{
    public class FileIO<T> where T : new()
    {
        private readonly string _path;

        public FileIO(string path) => _path = path;

        public T LoadData()
        {
            if (!File.Exists(_path))
            {
                CreatePath(_path);
                File.CreateText(_path).Dispose();
                T result = new T();
                SaveData(result);
                return result;
            }

            using StreamReader reader = File.OpenText(_path);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }

        public void SaveData(T db)
        {
            using StreamWriter writer = File.CreateText(_path);
            writer.Write(JsonConvert.SerializeObject(db, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            }));
        }
    }
}