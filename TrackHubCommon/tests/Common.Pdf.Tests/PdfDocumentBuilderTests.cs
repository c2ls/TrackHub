using System.Text;
using System.Text.RegularExpressions;
using Common.Pdf;
using FluentAssertions;

namespace Common.Pdf.Tests;

public class PdfDocumentBuilderTests
{
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAC0lEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static readonly Regex PageObjects = new(@"/Type\s*/Page(?![s])", RegexOptions.Compiled);

    [Fact]
    public void Build_ProducesAPdf()
    {
        var bytes = new PdfDocumentBuilder().Paragraph("Hello").Build();

        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    [Fact]
    public void Build_IsDeterministic()
    {
        byte[] Render() => Document(rows: 40).Build(new PdfMetadata { Title = "T", Created = new DateTime(2026, 8, 19, 6, 30, 0, DateTimeKind.Utc) });

        Render().Should().Equal(Render());
    }

    [Fact]
    public void LongTable_FlowsAcrossPages_RepeatingTheHeader()
    {
        var onePage = Document(rows: 5).Build();
        var manyPages = Document(rows: 400).Build();

        PageObjects.Count(Latin1(onePage)).Should().Be(1);
        PageObjects.Count(Latin1(manyPages)).Should().BeGreaterThan(3);
    }

    [Fact]
    public void Footer_ReceivesPageNumberAndCount()
    {
        var seen = new List<(int Page, int Total)>();

        Document(rows: 400).Footer((page, total) =>
        {
            seen.Add((page, total));
            return $"{page}/{total}";
        }).Build();

        seen.Should().NotBeEmpty();
        seen.Select(s => s.Page).Should().BeInAscendingOrder().And.OnlyHaveUniqueItems();
        seen.Should().OnlyContain(s => s.Total == seen.Count);
    }

    [Fact]
    public void Metadata_StampsOnlyWhatIsGiven()
    {
        var created = new DateTime(2026, 8, 19, 6, 30, 0, DateTimeKind.Utc);
        var stamped = Latin1(new PdfDocumentBuilder().Paragraph("x").Build(new PdfMetadata { Created = created }));
        var bare = Latin1(new PdfDocumentBuilder().Paragraph("x").Build());

        PdfDate.Parse(stamped, "CreationDate").Should().Be(new DateTimeOffset(created));
        stamped.Should().NotContain("/ModDate");
        bare.Should().NotContain("/CreationDate").And.NotContain("/ModDate");
    }

    [Fact]
    public void Text_IsEmbeddedAsSearchableGlyphs()
        => Latin1(new PdfDocumentBuilder().Paragraph("Hello").Build()).Should().Contain("/FontFile2");

    [Fact]
    public void UndecodableImage_IsSkipped()
    {
        var act = () => new PdfDocumentBuilder()
            .Image([1, 2, 3], 40)
            .Table(new PdfTable(2).Row("a", PdfCell.Picture([9, 9], 20)))
            .Build();

        act.Should().NotThrow();
    }

    [Fact]
    public void LongWord_IsBrokenRatherThanOverflowing()
    {
        var act = () => new PdfDocumentBuilder().Paragraph(new string('W', 2000)).Build();

        act.Should().NotThrow();
    }

    [Fact]
    public void Row_WithMoreCellsThanColumns_Throws()
    {
        var act = () => new PdfTable(2).Row("a", "b", "c");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("#344767", 0x34, 0x47, 0x67)]
    [InlineData("ff0000", 0xFF, 0, 0)]
    [InlineData("not-a-colour", 0, 0, 0)]
    [InlineData("#GGGGGG", 0, 0, 0)]
    [InlineData("", 0, 0, 0)]
    [InlineData(null, 0, 0, 0)]
    public void Color_Parse_FallsBackOnBadInput(string? hex, int r, int g, int b)
        => PdfColor.Parse(hex, PdfColor.Black).Should().Be(new PdfColor((byte)r, (byte)g, (byte)b));

    private static PdfDocumentBuilder Document(int rows)
    {
        var table = new PdfTable(3).HeaderRow("Name", "Value", "Photo");
        for (var i = 0; i < rows; i++)
        {
            if (i % 50 == 0)
            {
                table.SpanRow($"Group {i / 50}");
            }

            table.Row($"row {i}", new PdfCell(i.ToString(), Align: PdfAlign.Right), PdfCell.Picture(Png, 12));
        }

        return new PdfDocumentBuilder()
            .Image(Png, 40)
            .Paragraph("Acme", PdfTextStyle.Subtitle)
            .Paragraph("Report", PdfTextStyle.Title)
            .Paragraph("Pending", PdfTextStyle.Body with { Background = PdfColor.LightYellow })
            .Paragraph("A long paragraph " + string.Join(' ', Enumerable.Repeat("word", 80)), PdfTextStyle.Small)
            .Table(table)
            .Footer((page, total) => $"Page {page} of {total}");
    }

    private static string Latin1(byte[] pdf) => Encoding.Latin1.GetString(pdf);
}

/// <summary>Reads a <c>D:YYYYMMDDHHmmSS±HH'mm'</c> entry from the information dictionary as an instant.</summary>
public static class PdfDate
{
    private static readonly Regex Entry = new(@"/(\w+)\s*\(D:(\d{14})([+-Z])(\d{2})'(\d{2})'?\)", RegexOptions.Compiled);

    public static DateTimeOffset Parse(string pdf, string key)
    {
        var match = Entry.Matches(pdf).Single(m => m.Groups[1].Value == key);
        var local = DateTime.ParseExact(match.Groups[2].Value, "yyyyMMddHHmmss", null);
        var offset = match.Groups[3].Value == "Z"
            ? TimeSpan.Zero
            : TimeSpan.Parse($"{match.Groups[4].Value}:{match.Groups[5].Value}") * (match.Groups[3].Value == "-" ? -1 : 1);
        return new DateTimeOffset(local, offset);
    }
}
