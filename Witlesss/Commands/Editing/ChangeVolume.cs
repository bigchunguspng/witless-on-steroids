namespace Witlesss.Commands.Editing
{
    public class ChangeVolume : FileEditingCommand
    {
        private string _arg;

        public override void Run()
        {
            if (NothingToProcess()) return;

            if (Text.Contains(' '))
            {
                _arg = Text.Split(' ')[1];

                Bot.Download(FileID, Chat, out var path, out var type);

                SendResult(Memes.ChangeVolume(path, _arg), type);
                Log($"{Title} >> VOLUME [{_arg}]");
            }
            else
                Bot.SendMessage(Chat, VOLUME_MANUAL);
        }

        protected override string AudioFileName => SongNameOr($"{Sender} Sound Effect.mp3");
        protected override string VideoFileName => _arg.Length < 8 ? $"VOLUME-{ValidFileName(_arg)}.mp4" : "VERY-LOUD-ICE-CREAM.mp4";
    }
}