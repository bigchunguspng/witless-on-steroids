namespace Witlesss.Commands.Editing
{
    public class ChangeVolume : AudioVideoCommand
    {
        private string _arg;

        public override void Run()
        {
            if (NothingToProcess()) return;

            if (Text.Contains(' '))
            {
                _arg = Text.Split(' ')[1];

                Bot.Download(FileID, Chat, out var path, out var type);

                SendResult(Memes.ChangeVolume(path, _arg), type, VideoFilename, AudioFilename);
                Log($"{Title} >> VOLUME [{_arg}]");
            }
            else
                Bot.SendMessage(Chat, "чувак ты думал что-то здесь будет?"); //todo VOLUME_MANUAL
        }

        string AudioFilename() => SongNameOr($"{Sender} Sound Effect.mp3");
        string VideoFilename() => double.TryParse(_arg, out _) ? $"VOLUME-{_arg}.mp4" : "VERY-LOUD-ICE-CREAM.mp4";
    }
}