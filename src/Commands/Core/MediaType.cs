namespace Witlesss.Commands.Core;

public enum MediaType
{
    ///////// A: V: Motion:
    Photo, //    ✓          
    Stick, //    ✓          
    Audio, // ✓     ✓       
    Anime, //    ✓  ✓       video w/o  sound
    Video, // ✓  ✓  ✓       video with sound
    Round, // ~  ✓  ✓       video note -> specific cropping rules
    
    // todo merge with Video + Movie, send all videos as animation, since telegram sends them as video anyway
}