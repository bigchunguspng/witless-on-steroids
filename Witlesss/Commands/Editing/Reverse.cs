using System.Threading.Tasks;

namespace Witlesss.Commands.Editing
{
    public class Reverse : FileEditingCommand
    {
        protected override async Task Execute()
        {
            var (path, type) = await Bot.Download(FileID, Chat);
            
            SendResult(await Memes.Reverse(path), type);
            Log($"{Title} >> REVERSED [<<]");
        }
        
        protected override string AudioFileName => SongNameOr($"Kid Named {Sender}.mp3");
        protected override string VideoFileName { get; } = "piece_fap_reverse.mp4";
    }
}