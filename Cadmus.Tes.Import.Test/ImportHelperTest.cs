namespace Cadmus.Tes.Import.Test;

public sealed class ImportHelperTest
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("  a   b ", "a b")]
    [InlineData("N/A", null)]
    [InlineData(" n/a ", null)]
    public void FilterValue_Ok(string? value, string? expected)
    {
        Assert.Equal(expected, ImportHelper.FilterValue(value, false));
    }

    [Theory]
    [InlineData("36.9507789", 36.9507789)]
    [InlineData("36,9507789", 36.9507789)]
    [InlineData(" -14.5 ", -14.5)]
    public void TryParseDecimal_Valid_Ok(string value, double expected)
    {
        Assert.True(ImportHelper.TryParseDecimal(value, out double d));
        Assert.Equal(expected, d, 7);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.000,5")]
    public void TryParseDecimal_Invalid_False(string? value)
    {
        Assert.False(ImportHelper.TryParseDecimal(value, out _));
    }

    [Theory]
    [InlineData("YES", true)]
    [InlineData(" yes ", true)]
    [InlineData("NO", false)]
    [InlineData(null, false)]
    public void IsYes_Ok(string? value, bool expected)
    {
        Assert.Equal(expected, ImportHelper.IsYes(value));
    }

    [Fact]
    public void AppendToNote_Empty_NoSeparator()
    {
        Assert.Equal("### H\n\nx", ImportHelper.AppendToNote(null, "x", "H"));
    }

    [Fact]
    public void AppendToNote_NotEmpty_AppendsAfterBlankLine()
    {
        Assert.Equal("a\n\n### H\n\nx", ImportHelper.AppendToNote("a", "x", "H"));
        Assert.Equal("a\n\nx", ImportHelper.AppendToNote("a", "x"));
    }

    [Theory]
    [InlineData("high", 1)]
    [InlineData("Medium", 2)]
    [InlineData("low", 3)]
    [InlineData("other", 0)]
    [InlineData(null, 0)]
    public void LabelToRank_Ok(string? value, short expected)
    {
        Assert.Equal(expected, ImportHelper.LabelToRank(value));
    }
}
