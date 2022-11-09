using System.IO;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webp" output.jpg
    public class F_ToJPG : F_Base
    {
        public F_ToJPG(string input, string extension) : base(Path.ChangeExtension(input, extension))
        {
            AddInput(input);
        }
    }
}