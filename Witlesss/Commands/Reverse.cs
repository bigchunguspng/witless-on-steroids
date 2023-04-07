namespace Witlesss.Commands
{
    public class Reverse : AudioVideoCommand
    {
        public override void Run()
        {
            if (NothingToProcess()) return;
            
            Bot.Download(FileID, Chat, out string path, out var type);
            
            SendResult(Memes.Reverse(path), type, VideoFilename, AudioFilename);
            Log($"{Title} >> REVERSED [<<]");

            string AudioFilename() => SongNameOr($"Kid Named {Sender}.mp3");
            string VideoFilename() => "piece_fap_club-R.mp4";
        }
    }
}