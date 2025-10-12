namespace PF_Bot.Routing.Commands;

public enum MediaType
{
    ///////// A: V: Motion:
    Photo, //    ✓          
    Stick, //    ✓          
    Audio, // ✓     ✓       
    Anime, //    ✓  ✓       video w/o  sound
    Video, // ✓  ✓  ✓       video with sound
    Round, // ~  ✓  ✓       video note -> specific cropping rules
    Voice, // SEND ONLY
    Other, // SEND ONLY     document

    // todo merge with Video + Movie, send all videos as animation, since telegram sends them as video anyway
}