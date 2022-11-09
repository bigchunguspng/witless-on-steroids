namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -vf reverse -af areverse output.mp4
    // ffmpeg -i "input.mp3"             -af areverse output.mp3
    public class F_Reverse : F_Base
    {
        public F_Reverse(string input, MediaType type) : base(SetOutName(input, "-R"))
        {
            var v = type > MediaType.Audio;

            AddInput(input);
            AddWhen(v, "-vf",  "reverse");
            AddOptions("-af", "areverse");
            AddSizeFix(input, type);
        }
    }
}