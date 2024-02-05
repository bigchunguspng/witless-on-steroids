using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing;

#pragma warning disable CS4014

public class SliceAsync
{
    private static Bot Bot => Bot.Instance;

    private readonly MessageData Message;
    private readonly long Chat;
    private readonly string Title, FileID;

    public SliceAsync(MessageData message, string fileID)
    {
        FileID = fileID;
        Message = message;
        Chat = message.Chat;
        Title = message.Title;
    }

    public async Task RunAsync()
    {
        string path;
        int wait;
        var type = MediaType.Video;
        if (FileID.StartsWith("http"))
        {
            wait = Bot.PingChat(Chat, PLS_WAIT_RESPONSE[Random.Next(5)]);

            var task = new DownloadVideoTask(FileID, Message).RunAsync();
            await Bot.RunSafelyAsync(task, Chat, wait);

            path = task.Result;

            Bot.EditMessage(Chat, wait, XDDD(Pick(PROCESSING_RESPONSE)));
        }
        else
        {
            Bot.Download(FileID, Chat, out path, out type);

            wait = Bot.PingChat(Chat, XDDD(Pick(PROCESSING_RESPONSE)));
        }

        await Task.Delay(10); // we need this method to run asynchronously in both scenarios

        var result = Memes.Slice(path);

        Task.Run(() => Bot.DeleteMessage(Chat, wait));

        SendResult(result, type);
        Log($"{Title} >> SLICED [~/~]");
    }

    private void SendResult(string result, MediaType type)
    {
        using var stream = File.OpenRead(result);
        if      (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, MP4));
        else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, MP4));
        else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        else if (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, MP3));
    }

    private const string MP4 = "piece_fap_slice.mp4", MP3 = "sliced_by_piece_fap_bot.mp3";
}