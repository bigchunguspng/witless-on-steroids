using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Witlesss.Commands.Editing
{
    public class Crop : Scale
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

        protected override void Execute()
        {
            var input = _isShakeMode ? "/crop " + Text : Text;
            if (input.Contains(' '))
            {
                string[] log = null;
                var args = input.Split(' ').Skip(1).ToArray();

                if (_isShakeMode)
                {
                    var crop   = 1 < args.Length ? args[1] : "0.95";
                    var speed  = 2 < args.Length ? args[2] : "random(0)";
                    var offset = 3 < args.Length ? args[3] : "random(0)";
                    args = F_Shake(crop, speed, offset).Split();
                    log  = new[] { crop, speed, offset };
                }

                for (var i = 0; i < Math.Min(args.Length, 4); i++)
                {
                    var w   = Regex.Replace(args[i], "(?<=[^io_]|^)w",           "iw");
                    args[i] = Regex.Replace(w,       "(?<=[^io_]|^)h(?=[^s]|$)", "ih");
                }

                if (args.Length > 4) args = args.Take(4).ToArray();

                Bot.Download(FileID, Chat, out var path, out var type);

                SendResult(Memes.Crop(path, args), type);
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