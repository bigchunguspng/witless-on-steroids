namespace Witlesss.Commands
{
    public class Reverse : RemoveBitrate
    {
        public override void Run()
        {
            if (NothingToProcess()) return;
            
            Bot.Download(FileID, Chat, out string path, out var type);
            
            string result = Memes.Reverse(path);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> REVERSED [<<]");

            string AudioFilename() => MediaFileName($"Kid Named {Sender}.mp3");
            string VideoFilename() => "piece_fap_club-R.mp4";
        }
    }
}