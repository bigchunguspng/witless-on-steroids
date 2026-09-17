using System.Text;

namespace PF_Bot.Backrooms;

public static partial class Extensions
{
    private const int TG_MAX_MESSAGE_LEN = 4096; // TEXT only, HTML tags are excluded.

    private const StringSplitOptions
        SPLIT_RM_EMPTY_TRIM = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    public static string[] SplitIntoPages
        (this string content, int offset_oneshot = 0, int offset_paginated = 0)
    {
        if (TG_MAX_MESSAGE_LEN >= offset_oneshot + content.Length)
            return [content]; // 92.3% exit here (386/418 files)

        if (TG_MAX_MESSAGE_LEN >= offset_oneshot + Length_ExcludingHTML(content))
            return [content]; //  4.1% here (17)

        // code for the rest 15  (3.6%) long ass files:
        // TL;DR:
        // Walk content line by line, keeping track of TEXT length and open tags.
        // When too long - split at line break.

        var sb = new StringBuilder();
        List <string> pages = []; // result
        Stack<string> tagsP = []; // breadcrumb of open tags, as for prev line
        Stack<string> tagsC = []; // breadcrumb of open tags, as for curr line
        var text_lengthP = 0; // of the current page, as for prev line
        var text_lengthC = 0; // of the current page, as for curr line
        var line_offset  = 0; // HTML, current line, chars
        var lines_paged  = 0; // first N lines assigned to pages
        var lines = content.Split('\n');
        var    curr_line = 0; // index
        while (curr_line < lines.Length)
        {
            // line can be: empty, [text & tag]*N & text?  | N>=0
            var line = lines[curr_line];
            if (line.Length - line_offset <= 0) // empty line / to text after tag
                goto LINE_END;

            // line not empty
            var i_tag = line.IndexOf('<', line_offset);
            if (i_tag < 0) // just text
            {
                text_lengthC += line.Length - line_offset;
                goto LINE_END;
            }

            // tag found
            var i_tag_end = line.IndexOf('>', i_tag + 1); // should be found!
            var tag_is_closing_part = line[i_tag     + 1] == '/';
            var tag_is_closing_full = line[i_tag_end - 1] == '/';
            var tag_is_opening_part = tag_is_closing_part.Janai() && tag_is_closing_full.Janai();

            // count length of text before tag
            text_lengthC += i_tag - line_offset;
            line_offset = i_tag_end + 1;

            // update tag breadcrumb
            if      (tag_is_closing_part) tagsC.Pop();
            else if (tag_is_opening_part)
            {
                var tag = line.Substring(i_tag + 1, i_tag_end - i_tag - 1);
                tagsC.Push(tag); // save THE WHOLE tag
            }

            continue;
            LINE_END:
            if (offset_paginated + text_lengthC > TG_MAX_MESSAGE_LEN)
            {
                // write the page
                var lines_to_take = curr_line - lines_paged;
                var page_lines = lines.Skip(lines_paged).Take(lines_to_take);
                sb.AppendJoin('\n', page_lines);
                foreach (var tag in tagsP)
                {
                    // close tags
                    var name = tag.Split(' ', 2, SPLIT_RM_EMPTY_TRIM)[0];
                    sb.Append("</").Append(name).Append('>');
                }
                pages.Add(sb.ToString());

                lines_paged += lines_to_take;
                text_lengthC -= text_lengthP;

                // start the next page
                sb.Clear();
                sb.Append("\n\n");
                foreach (var tag in tagsP.Reverse())
                {
                    // open tags on the next page
                    sb.Append('<').Append(tag).Append('>');
                }
            }
            text_lengthC++; // \n
            text_lengthP = text_lengthC;
            curr_line++;
            line_offset = 0;
            tagsP = new Stack<string>(tagsC.Reverse());
        }

        // write the last page
        sb.AppendJoin('\n', lines.Skip(lines_paged));
        foreach (var tag in tagsP)
        {
            // close tags
            var name = tag.Split(' ', 2, SPLIT_RM_EMPTY_TRIM)[0];
            sb.Append("</").Append(name).Append('>');
        }
        pages.Add(sb.ToString());

        return pages.ToArray();
    }

    private static readonly Regex
        _r_tag = new("""<.+?>""", RegexOptions.Compiled);

    public static int Length_ExcludingHTML
        (this string text) => text.Length - _r_tag.Matches(text).Sum(x => x.Length);
}