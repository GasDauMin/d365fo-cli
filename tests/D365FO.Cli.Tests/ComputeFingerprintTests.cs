using D365FO.Cli.Commands.Index;

namespace D365FO.Cli.Tests;

/// <summary>
/// Verifies that <see cref="IndexExtractCommand.ComputeFingerprint"/> is
/// sensitive to both label-file changes and LabelLanguages config changes.
/// </summary>
public class ComputeFingerprintTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"d365fo-fp-{Guid.NewGuid():N}");

    public ComputeFingerprintTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }

    [Fact]
    public void Fingerprint_changes_when_label_txt_added()
    {
        var fp1 = IndexExtractCommand.ComputeFingerprint(_dir);
        File.WriteAllText(Path.Combine(_dir, "SysLabel.en-US.label.txt"), "Title=Hello");
        var fp2 = IndexExtractCommand.ComputeFingerprint(_dir);

        Assert.NotEqual(fp1, fp2);
    }

    [Fact]
    public void Fingerprint_changes_when_label_txt_modified()
    {
        var txt = Path.Combine(_dir, "SysLabel.en-US.label.txt");
        File.WriteAllText(txt, "Title=Hello");
        var fp1 = IndexExtractCommand.ComputeFingerprint(_dir);

        // Force mtime change (some FS have 1-s resolution)
        File.SetLastWriteTimeUtc(txt, File.GetLastWriteTimeUtc(txt).AddSeconds(2));
        var fp2 = IndexExtractCommand.ComputeFingerprint(_dir);

        Assert.NotEqual(fp1, fp2);
    }

    [Fact]
    public void Fingerprint_changes_when_LabelLanguages_extended()
    {
        File.WriteAllText(Path.Combine(_dir, "SysLabel.en-US.label.txt"), "Title=Hello");

        var fp1 = IndexExtractCommand.ComputeFingerprint(_dir, new[] { "en-us" });
        var fp2 = IndexExtractCommand.ComputeFingerprint(_dir, new[] { "en-us", "cs" });

        Assert.NotEqual(fp1, fp2);
    }

    [Fact]
    public void Fingerprint_stable_when_nothing_changes()
    {
        File.WriteAllText(Path.Combine(_dir, "SysLabel.en-US.label.txt"), "Title=Hello");
        var langs = new[] { "en-us", "cs" };

        var fp1 = IndexExtractCommand.ComputeFingerprint(_dir, langs);
        var fp2 = IndexExtractCommand.ComputeFingerprint(_dir, langs);

        Assert.Equal(fp1, fp2);
    }

    [Fact]
    public void Fingerprint_langs_order_insensitive()
    {
        var fp1 = IndexExtractCommand.ComputeFingerprint(_dir, new[] { "en-us", "cs" });
        var fp2 = IndexExtractCommand.ComputeFingerprint(_dir, new[] { "cs", "en-us" });

        Assert.Equal(fp1, fp2);
    }
}
