using System.IO;
using Newtonsoft.Json;

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
                Directory.CreateDirectory(_path.Remove(_path.LastIndexOf('\\')));
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