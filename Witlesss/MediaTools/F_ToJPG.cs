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
}