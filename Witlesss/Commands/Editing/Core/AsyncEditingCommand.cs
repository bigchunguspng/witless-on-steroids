using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;

#pragma warning disable CS4014

namespace Witlesss.Commands.Editing.Core
{
    public abstract class AsyncEditingCommand : AsyncCommand
    {
        protected readonly string FileID;

        protected int WaitMessage;
        protected string Path;
        protected MediaType Type;

        protected AsyncEditingCommand(MessageData message, string fileID) : base(message)
        {
            FileID = fileID;
        }

        protected async Task DownloadFileAsync()
        {
            if (FileID.StartsWith("http"))
            {
                WaitMessage = Bot.PingChat(Chat, PLS_WAIT_RESPONSE[Random.Next(5)]);

                var task = new DownloadVideoTask(FileID, MessageData).RunAsync();
                await Bot.RunSafelyAsync(task, Chat, WaitMessage);

                Path = task.Result;
                Type = MediaType.Video;

                Bot.EditMessage(Chat, WaitMessage, XDDD(Pick(PROCESSING_RESPONSE)));
            }
            else
            {
                Bot.Download(FileID, Chat, out Path, out Type);

                if (SizeInBytes(Path) > 4_000_000)
                {
                    await Task.Delay(10);
                    WaitMessage = Bot.PingChat(Chat, XDDD(Pick(PROCESSING_RESPONSE)));
                }
            }
        }
    
        protected void SendResult(string result, MediaType type)
        {
            using var stream = File.OpenRead(result);
            if      (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, MP4));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, MP4));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            else if (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, MP3));
        }

        protected abstract string MP3 { get; }
        protected abstract string MP4 { get; }
    
        protected string Sender => ValidFileName(GetSenderName(Message));
        protected string SongNameOr(string s) => Extension.SongNameOr(Message, s);
    }
}