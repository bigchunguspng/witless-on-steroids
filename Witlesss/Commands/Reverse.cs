using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class Reverse : RemoveBitrate
    {
        public override void Run()
        {
            string fileID = GetVideoOrAudioID();
            if (fileID == null) return;
            
            Download(fileID, out string path, out var type);
            
            string result = Bot.MemeService.Reverse(path);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> REVERSED [<<]");

            string AudioFilename() => $"{ReverseText(ValidFileName(SenderName(Message)))}, lol.mp3";
            string VideoFilename() => "piece_fap_club-R.mp4";
        }
    }
}