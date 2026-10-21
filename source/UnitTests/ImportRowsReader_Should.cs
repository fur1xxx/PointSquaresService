using System.Text.Json;
using Domain.Core;
using PointSquaresService.Endpoints.Requests;
using Xunit;

namespace UnitTests;

public class ImportRowsReader_Should
{
    [Fact]
    public void Read_GoodRows_ReturnsPointsInRowOrder()
    {
        var rows = Read("""[{"x":1,"y":2},{"x":-3,"y":4},{"x":0,"y":0}]""");

        Assert.Equal([new Point(1, 2), new Point(-3, 4), new Point(0, 0)], rows.Points);
        Assert.Equal(3, rows.Received);
        Assert.Equal(0, rows.Excluded);
        Assert.Empty(rows.ExcludedRows);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("7")]
    [InlineData("[1,2]")]
    [InlineData("\"a\"")]
    public void Read_RowNotAnObject_ExcludesIt(string row)
    {
        var excludedRow = ReadSingleExcludedRow(row);

        Assert.Equal(new ImportRows.ExcludedRow(1, null, null, "Row must be an object with x and y"), excludedRow);
    }

    [Theory]
    [InlineData("""{"y":1}""")]
    [InlineData("""{"x":null,"y":1}""")]
    public void Read_XMissingOrNull_ExcludesItAsMissing(string row)
    {
        Assert.Equal("X is missing", ReadSingleExcludedRow(row).Reason);
    }

    [Fact]
    public void Read_YMissing_ExcludesItAsMissing()
    {
        Assert.Equal("Y is missing", ReadSingleExcludedRow("""{"x":1}""").Reason);
    }

    [Theory]
    [InlineData("\"5\"")]
    [InlineData("1.5")]
    [InlineData("1.0")]
    [InlineData("1e3")]
    [InlineData("true")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void Read_XNotAWholeNumber_ExcludesIt(string x)
    {
        Assert.Equal("X must be a whole number", ReadSingleExcludedRow($$"""{"x":{{x}},"y":1}""").Reason);
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    [InlineData("99999999999999999999")]
    public void Read_XOutsideIntRange_ExcludesIt(string x)
    {
        Assert.Equal("X is out of range", ReadSingleExcludedRow($$"""{"x":{{x}},"y":1}""").Reason);
    }

    [Fact]
    public void Read_IntRangeEnds_AcceptsThem()
    {
        var rows = Read("""[{"x":-2147483648,"y":2147483647}]""");

        Assert.Equal([new Point(int.MinValue, int.MaxValue)], rows.Points);
    }

    [Fact]
    public void Read_XAndYBothBad_ReportsX()
    {
        Assert.Equal("X must be a whole number", ReadSingleExcludedRow("""{"x":1.5,"y":"a"}""").Reason);
    }

    [Fact]
    public void Read_SamePointTwice_KeepsFirstAndExcludesSecondAsDuplicateOfRow1()
    {
        var rows = Read("""[{"x":1,"y":2},{"x":1,"y":2}]""");

        Assert.Equal([new Point(1, 2)], rows.Points);
        var excludedRow = Assert.Single(rows.ExcludedRows);
        Assert.Equal(2, excludedRow.Row);
        Assert.Equal("Duplicate of row 1", excludedRow.Reason);
    }

    [Fact]
    public void Read_DuplicateAfterBadRow_PointsToFirstGoodRow()
    {
        var rows = Read("""[{"x":1.5,"y":2},{"x":1,"y":2},{"x":1,"y":2}]""");

        Assert.Equal([1, 3], rows.ExcludedRows.Select(row => row.Row));
        Assert.Equal("Duplicate of row 2", rows.ExcludedRows[1].Reason);
    }

    [Fact]
    public void Read_UpperCaseNamesAndExtraProperties_AcceptsRow()
    {
        var rows = Read("""[{"X":1,"Y":2,"z":3}]""");

        Assert.Equal([new Point(1, 2)], rows.Points);
    }

    [Fact]
    public void Read_1001BadRows_Lists1000AndCountsAll()
    {
        var rows = Read($"[{string.Join(",", Enumerable.Repeat("7", 1_001))}]");

        Assert.Equal(1_001, rows.Received);
        Assert.Equal(1_001, rows.Excluded);
        Assert.Equal(1_000, rows.ExcludedRows.Count);
        Assert.Equal(1_000, rows.ExcludedRows[^1].Row);
    }

    [Fact]
    public void Read_BadRow_EchoesValuesWithTheirTypes()
    {
        var rows = Read("""[{"x":"abc","y":5},{"x":null,"y":5}]""");

        Assert.Equal(
            """[{"row":1,"x":"abc","y":5,"reason":"X must be a whole number"},{"row":2,"x":null,"y":5,"reason":"X is missing"}]""",
            JsonSerializer.Serialize(rows.ExcludedRows, JsonSerializerOptions.Web)
        );
    }

    [Fact]
    public void Read_LongValues_EchoesFirst50Characters()
    {
        var longString = new string('a', 60);
        var longNumber = new string('1', 60);

        var rows = Read($$"""[{"x":"{{longString}}","y":1},{"x":{{longNumber}},"y":1}]""");

        Assert.Equal(new string('a', 50), rows.ExcludedRows[0].X!.Value.GetString());
        Assert.Equal(new string('1', 50), rows.ExcludedRows[1].X!.Value.GetString());
    }

    private static ImportRows Read(string json) => ImportRowsReader.Read(JsonSerializer.Deserialize<JsonElement>(json));

    private static ImportRows.ExcludedRow ReadSingleExcludedRow(string row)
    {
        var rows = Read($"[{row}]");

        Assert.Empty(rows.Points);
        Assert.Equal(1, rows.Excluded);
        return Assert.Single(rows.ExcludedRows);
    }
}
