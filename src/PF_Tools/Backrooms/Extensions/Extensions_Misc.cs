using HtmlAgilityPack;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Misc
{
    public static async Task DownloadFileAsync
        (this HttpClient client, string url, string path)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using var fs = File.Create(path);
        await response.Content.CopyToAsync(fs);
    }

    public static IEnumerable<HtmlNode> ElementChildren
        (this HtmlNode node) => node.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element);

    public static HtmlNode? PrevElementSibling(this HtmlNode? node)
    {
        node = node?.PreviousSibling;
        while (node != null && node.NodeType != HtmlNodeType.Element) node = node.PreviousSibling;
        return node;
    }
    public static HtmlNode? NextElementSibling(this HtmlNode? node)
    {
        node = node?.NextSibling;
        while (node != null && node.NodeType != HtmlNodeType.Element) node = node.NextSibling;
        return node;
    }
    public static HtmlNode? FirstElementChild(this HtmlNode? node)
    {
        node = node?.FirstChild;
        while (node != null && node.NodeType != HtmlNodeType.Element) node = node.NextSibling;
        return node;
    }
}