using PF_Bot.Core;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages.Commands;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

public class FFMpegDocs_Callback : CallbackHandler
{
    protected override Task Run()
    {
        var ops = Key[Registry.CallbackKey_FFMpeg.Length..];
        var audio = ops.StartsWith('a');
        var video = ops.StartsWith('v');
        var filters = ops.EndsWith('f');
        if (audio || video)
        {
            var kind = audio ? FilterKind.Audio : FilterKind.Video;
            if (filters)
            {
                ListingFFMpegDocs.SendFilters(kind, GetPagination(Content));
            }
            else
            {
                var bits = Content.Split(':', 2);
                var page = bits.Length > 1 ? int.Parse(bits[1]) : 0;
                var code =                   int.Parse(bits[0]) + 1;
                ListingFFMpegDocs.SendPage_OrSyntax(Origin, kind, code, page, Message.Id);
            }
        }
        else if (Content == "m") ListingFFMpegDocs.SendMainMenu(Origin, Message.Id);
        else if (Content == "x") ListingFFMpegDocs.SendSyntax  (Origin, Message.Id);

        return Task.CompletedTask;
    }
}

public class FFMpegDocs : CommandHandlerBlocking
{
    protected override void Run()
    {
        if (Args == null) ListingFFMpegDocs.SendMainMenu(Origin);
        else
        {
            var bits = Args.ToLower().SplitN(2);
            var bit1 = bits[0];
            var bit2 = bits.Length > 1 ? bits[1] : null;
            var a = bit1 is "a";
            var v = bit1 is "v";
            var x = bit1 is "x";
            if (x)
            {
                ListingFFMpegDocs.SendSyntax(Origin);
            }
            else if (a || v)
            {
                var kind = a ? FilterKind.Audio : FilterKind.Video;
                var target = int.TryParse(bit2, out var value) ? value : (int?)null;
                var pagination = new ListPagination(Origin, PerPage: ListingFFMpegDocs.PER_PAGE);
                ListingFFMpegDocs.SendFilters(kind, pagination, target);
            }
            else // find exact page
            {
                var m_av67 = _r_code_av67.Match(bit1);
                var m_67av = _r_code_67av.Match(bit1);
                if (m_av67.Success || m_67av.Success)
                {
                    var kind_src = m_av67.Success
                        ? m_av67.Groups[1].Value
                        : m_67av.Groups[2].Value;
                    var code_src = m_av67.Success
                        ? m_av67.Groups[2].Value
                        : m_67av.Groups[1].Value;
                    var kind = kind_src == "a" ? FilterKind.Audio : FilterKind.Video;
                    var code = int.Parse(code_src);
                    ListingFFMpegDocs.SendPage_OrSyntax(Origin, kind, code);
                }
                else
                {
                    ListingFFMpegDocs.SendPage_OrSyntax(Origin, bit1); // find by text
                }
            }
        }
    }

    private static readonly Regex
        _r_code_av67 = new(@"([av])(\d+)", RegexOptions.Compiled),
        _r_code_67av = new(@"(\d+)([av])", RegexOptions.Compiled);
}