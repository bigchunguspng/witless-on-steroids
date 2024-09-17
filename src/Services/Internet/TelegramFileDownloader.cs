using System.Diagnostics.CodeAnalysis;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Witlesss.Services.Internet
{
    public class TelegramFileDownloader
    {
        /// <summary>
        /// Downloads a file to the <b>Pics</b> directory.
        /// Grabs recent and large files from cache.
        /// To be used with media files attached to commands.
        /// </summary>
        public async Task<string> Download(FileBase file, long chat, string extension)
        {
            var directory = Path.Combine(Dir_Pics, chat.ToString());
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"{file.FileUniqueId}{extension}");
            if (File.Exists(path))
            {
                while (FileIsLocked(path)) await Task.Delay(250);
            }
            else
            {
                await DownloadFile(file.FileId, path, chat);
            }

            return path;
        }

        /// <summary>
        /// Downloads a file or send a message if the exception is thrown.
        /// Make sure provided path is unique and directory is created.
        /// </summary>
        public async Task DownloadFile(string fileId, string path, long chat = default)
        {
            try
            {
                var file = await Bot.Instance.Client.GetFileAsync(fileId);
                var stream = new FileStream(path, FileMode.Create);
                Bot.Instance.Client.DownloadFileAsync(file.FilePath!, stream).Wait();
                await stream.DisposeAsync();
            }
            catch (Exception e)
            {
                var message = e.Message.Contains("file is too big") 
                    ? FILE_TOO_BIG.PickAny() 
                    : e.Message.XDDD();
                Bot.Instance.SendMessage(chat, message);
                throw;
            }
        }

        private static bool FileIsLocked(string path)
        {
            try
            {
                using var fs = File.Open(path, FileMode.Open, FileAccess.Read);
                fs.Close();
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
    }

    public class LimitedCache<TKey, TValue> where TKey : notnull
    {
        private readonly int _limit;

        private readonly Queue<TKey> _keys;
        private readonly Dictionary<TKey, TValue> _paths;

        public LimitedCache(int limit)
        {
            _limit = limit;
            _keys = new Queue<TKey>(_limit);
            _paths = new Dictionary<TKey, TValue>(_limit);
        }

        public void Add(TKey id, TValue value)
        {
            if (_keys.Count == _limit)
            {
                var key = _keys.Dequeue();
                _paths.Remove(key);
            }
            _keys.Enqueue(id);
            _paths.TryAdd(id, value);
        }

        public bool Contains(TKey id, [NotNullWhen(true)] out TValue? value)
        {
            return _paths.TryGetValue(id, out value);
        }
    }
}