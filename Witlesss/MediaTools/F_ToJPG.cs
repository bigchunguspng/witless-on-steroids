using static System.IO.Path;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webp" output.jpg
    public class F_ToJPG : F_Base
    {
        public F_ToJPG(string input, string extension) : base(SetOutName(ChangeExtension(input, extension), "-W"))
        {
            AddInput(input);
        }
    }

    //ffmpeg -i "input.png" -qscale:v 7 output.jpg
    public class F_CompressImage : F_ToJPG
    {
        public F_CompressImage(string input) : base(input, ".jpg")
        {
            AddOptions("-qscale:v", "5");
        }
    }
}