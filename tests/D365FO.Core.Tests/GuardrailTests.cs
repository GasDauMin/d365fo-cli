using D365FO.Core;
using Xunit;

namespace D365FO.Core.Tests;

public class StringSanitizerTests
{
    [Fact]
    public void Preserves_printable_and_newlines()
    {
        Assert.Equal("hello\nworld\t!", StringSanitizer.Sanitize("hello\nworld\t!"));
    }

    [Fact]
    public void Strips_control_characters()
    {
        var input = "safe\u0001text\u0007here";
        Assert.Equal("safetexthere", StringSanitizer.Sanitize(input));
    }

    [Fact]
    public void Null_passthrough()
    {
        Assert.Null(StringSanitizer.Sanitize(null));
    }

    [Theory]
    [InlineData("\u200BHello")]        // zero-width space
    [InlineData("Hello\u200F")]        // right-to-left mark
    [InlineData("\u202EHello")]        // right-to-left override (classic "evil" char)
    [InlineData("\uFEFFHello")]        // BOM / zero-width no-break space
    [InlineData("Hel\u2060lo")]        // word joiner
    [InlineData("He\u200Bllo\u202E")] // combination
    public void Strips_unicode_format_chars_used_for_injection(string input)
    {
        var result = StringSanitizer.Sanitize(input);
        // None of the injected chars should survive
        Assert.DoesNotContain('\u200B', result!);
        Assert.DoesNotContain('\u200F', result!);
        Assert.DoesNotContain('\u202E', result!);
        Assert.DoesNotContain('\uFEFF', result!);
        Assert.DoesNotContain('\u2060', result!);
        // Printable content must be preserved
        Assert.Contains("Hello", result!);
    }

    [Fact]
    public void Preserves_content_after_format_char_removal()
    {
        // Simulates a label where a BOM was prepended and a direction override appended
        var input = "\uFEFFVehicle Identification Number\u202E";
        Assert.Equal("Vehicle Identification Number", StringSanitizer.Sanitize(input));
    }
}

public class ToolResultTests
{
    [Fact]
    public void Success_has_ok_true_and_no_error()
    {
        var r = ToolResult<int>.Success(42);
        Assert.True(r.Ok);
        Assert.Equal(42, r.Data);
        Assert.Null(r.Error);
    }

    [Fact]
    public void Fail_sets_code_and_message()
    {
        var r = ToolResult<int>.Fail("X", "m", "h");
        Assert.False(r.Ok);
        Assert.Equal("X", r.Error!.Code);
        Assert.Equal("m", r.Error.Message);
        Assert.Equal("h", r.Error.Hint);
    }
}
