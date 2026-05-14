namespace D365FO.Core;

/// <summary>
/// Sanitizes free-form strings sourced from the D365FO metadata index
/// (labels, descriptions) before they are rendered back to an LLM caller.
/// Protects against prompt-injection attempts embedded in customer data.
/// Callers can opt out with a --raw flag in the CLI layer.
/// </summary>
public static class StringSanitizer
{
    /// <summary>
    /// Pattern covering Unicode format characters commonly abused for prompt
    /// injection in LLM contexts:
    /// <list type="bullet">
    ///   <item>U+200B–U+200F — zero-width spaces / direction marks</item>
    ///   <item>U+202A–U+202E — bidirectional embedding / override controls</item>
    ///   <item>U+2060–U+2069 — word joiner, invisible separators, isolate controls</item>
    ///   <item>U+FEFF        — byte order mark / zero-width no-break space</item>
    /// </list>
    /// These characters are invisible in most UIs but interpreted by LLM
    /// tokenisers and can be used to smuggle hidden instructions.
    /// </summary>
    private static readonly System.Text.RegularExpressions.Regex UnicodeFormatChars =
        new(@"[\u200B-\u200F\u202A-\u202E\u2060-\u2069\uFEFF]",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    public static string? Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // First pass: strip Unicode bidirectional/format characters that are
        // invisible in most editors but can mislead LLM tokenisers.
        value = UnicodeFormatChars.Replace(value, string.Empty);

        // Second pass: strip ASCII control characters (0x00–0x1F) except
        // newline, carriage-return, and horizontal tab which are legitimate
        // in multi-line labels and descriptions.
        var span = value.AsSpan();
        var buf = new System.Text.StringBuilder(span.Length);
        foreach (var ch in span)
        {
            if (ch < 0x20 && ch is not ('\n' or '\r' or '\t')) continue;
            buf.Append(ch);
        }
        return buf.ToString();
    }
}
