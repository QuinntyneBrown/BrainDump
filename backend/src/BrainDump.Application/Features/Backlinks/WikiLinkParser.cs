using System.Text.RegularExpressions;

namespace BrainDump.Application.Features.Backlinks;

public abstract record ParsedLink
{
    public sealed record ById(int TargetId) : ParsedLink;
    public sealed record ByTitle(string Title) : ParsedLink;
}

/// <summary>
/// Pure parser for [[wiki-style]] document references. Two forms:
///   [[id:42]]              — exact target id
///   [[Document Title]]     — exact case-insensitive title match
/// </summary>
public static class WikiLinkParser
{
    private static readonly Regex Pattern = new(
        @"\[\[(?<body>[^\[\]]+)\]\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IEnumerable<ParsedLink> Extract(string? text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        foreach (Match match in Pattern.Matches(text))
        {
            var body = match.Groups["body"].Value.Trim();
            if (body.Length == 0) continue;

            if (body.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                var idStr = body[3..].Trim();
                if (int.TryParse(idStr, out var id) && id > 0)
                    yield return new ParsedLink.ById(id);
                continue;
            }

            yield return new ParsedLink.ByTitle(body);
        }
    }

    public static IEnumerable<ParsedLink> ExtractMany(IEnumerable<string?> sources)
    {
        foreach (var source in sources)
        foreach (var link in Extract(source))
            yield return link;
    }
}
