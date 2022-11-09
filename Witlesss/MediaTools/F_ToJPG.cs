using System.IO;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webp" output.jpg
    public class F_ToJPG : F_SimpleTask
    {
        public F_ToJPG(string input, string extension) : base(input, Path.ChangeExtension(input, extension))
        {
            AddInput(input);
        }
    }
}