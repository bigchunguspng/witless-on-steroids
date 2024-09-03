namespace Witlesss.Commands.Editing
{
    public class Crop : VideoPhotoCommand // todo shake is only for video
    {
        private const string _crop  = "piece_fap_crop.mp4";
        private const string _shake = "shake_fap_club.mp4";

        private bool _isShakeMode; 

        public Crop UseShakeMode()
        {
            _isShakeMode = true;
            return this;
        }

        public Crop UseDefaultMode()
        {
            _isShakeMode = false;
            return this;
        }

        protected override string SyntaxManual => "/man_crop";

        protected override async Task Execute()
        {
            if (Args is not null || _isShakeMode)
            {
                string[]? log = null;
                var args = Args?.Split(' ');

                if (_isShakeMode)
                {
                    var crop   = args?.Length > 0 ? args[0] : "0.95";
                    var speed  = args?.Length > 1 ? args[1] : "random(0)";
                    var offset = args?.Length > 2 ? args[2] : "random(0)";
                    args = F_Shake(crop, speed, offset).Split();
                    log  = [crop, speed, offset];
                }

                for (var i = 0; i < Math.Min(args!.Length, 4); i++)
                {
                    var w   = Regex.Replace(args[i], "(?<=[^io_]|^)w",           "iw");
                    args[i] = Regex.Replace(w,       "(?<=[^io_]|^)h(?=[^s]|$)", "ih");
                }

                if (args.Length > 4) args = args.Take(4).ToArray();

                var path = await Bot.Download(FileID, Chat, Ext);

                var input = path.UseFFMpeg();
                var process = Ext is ".jpg" ? input.CropJpeg(args) : input.CropVideo(args);
                SendResult(await process.Out("-crop", Ext));
                Log($"{Title} >> {CropOrShake} [{string.Join(':', _isShakeMode ? log! : args)}]");
            }
            else
                Bot.SendMessage(Chat, CROP_MANUAL);
        }

        protected override string VideoFileName => _isShakeMode ? _shake : _crop;

        private string CropOrShake => _isShakeMode ? "SHAKE" : "CROP";

        private static string F_Shake(string a, string b, string c)
        {
            return $"w*({a}) h*({a}) (w-out_w)/2+((w-out_w)/2)*sin(t*({b})-{c}) (h-out_h)/2+((h-out_h)/2)*sin(t*({b})+2*{c})";
        }
    }
}