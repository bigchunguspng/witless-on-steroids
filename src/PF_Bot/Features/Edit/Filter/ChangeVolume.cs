using PF_Bot.Features.Edit.Core;

namespace PF_Bot.Features.Edit.Filter
{
    public class ChangeVolume : AudioVideoCommand
    {
        private string _arg = null!;

        protected override string SyntaxManual => "/man_vol";

        protected override async Task Execute()
        {
            if (Args is null)
            {
                Bot.SendMessage(Origin, VOLUME_MANUAL);
            }
            else
            {
                _arg = Args.Split(' ', 2)[0];

                var path = await DownloadFile();

                SendResult(await path.UseFFMpeg(Origin).ChangeVolume(_arg).Out("-vol", Ext));
                Log($"{Title} >> VOLUME [{_arg}]");
            }
        }

        protected override string AudioFileName => SongNameOr($"{Sender} Sound Effect.mp3");
        protected override string VideoFileName => _arg.Length < 8 ? $"VOLUME-{_arg.ValidFileName()}.mp4" : "VERY-LOUD-ICE-CREAM.mp4";
    }
}