namespace Witlesss.Commands.Core;

public enum MediaType
{
    ///////// A: V: Motion:
    Photo, //    ✓          
    Stick, //    ✓          
    Audio, // ✓     ✓       
    Video, //    ✓  ✓       video w/o  sound
    Movie, // ✓  ✓  ✓       video with sound
    Round, // ~  ✓  ✓       video note -> specific cropping rules
    
    // todo merge with Video + Movie, send all videos as animation, since telegram sends them as video anyway
}