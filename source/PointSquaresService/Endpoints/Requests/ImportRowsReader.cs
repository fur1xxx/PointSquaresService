using System.Text.Json;
using Domain.Core;

namespace PointSquaresService.Endpoints.Requests;

public static class ImportRowsReader
{
    private const int MaxExcludedRowsListed = 1_000;
    private const int MaxEchoedLength = 50;

    public static ImportRows Read(JsonElement rows)
    {
        var points = new List<Point>();
        var firstRowOfPoint = new Dictionary<Point, int>();
        var excludedRows = new List<ImportRows.ExcludedRow>();
        var excluded = 0;
        var row = 0;

        foreach (var element in rows.EnumerateArray())
        {
            row++;
            var isObject = element.ValueKind == JsonValueKind.Object;
            var x = isObject ? FindProperty(element, "x") : null;
            var y = isObject ? FindProperty(element, "y") : null;
            var reason = isObject ? CheckCoordinate(x, "X") ?? CheckCoordinate(y, "Y") : "Row must be an object with x and y";

            if (reason is null)
            {
                var point = new Point(x!.Value.GetInt32(), y!.Value.GetInt32());
                if (firstRowOfPoint.TryAdd(point, row))
                {
                    points.Add(point);
                    continue;
                }
                reason = $"Duplicate of row {firstRowOfPoint[point]}";
            }

            excluded++;
            if (excludedRows.Count < MaxExcludedRowsListed)
                excludedRows.Add(new ImportRows.ExcludedRow(row, Echo(x), Echo(y), reason));
        }

        return new ImportRows(row, points, excluded, excludedRows);
    }

    private static JsonElement? FindProperty(JsonElement row, string name)
    {
        foreach (var property in row.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return property.Value;
        return null;
    }

    private static string? CheckCoordinate(JsonElement? value, string name) =>
        value switch
        {
            null or { ValueKind: JsonValueKind.Null } => $"{name} is missing",
            { ValueKind: not JsonValueKind.Number } => $"{name} must be a whole number",
            { } number when number.GetRawText().AsSpan().ContainsAny('.', 'e', 'E') => $"{name} must be a whole number",
            { } number when !number.TryGetInt32(out _) => $"{name} is out of range",
            _ => null,
        };

    private static JsonElement? Echo(JsonElement? value)
    {
        if (value is not { } element)
            return null;

        var text = element.ValueKind == JsonValueKind.String ? element.GetString()! : element.GetRawText();
        return text.Length > MaxEchoedLength ? JsonSerializer.SerializeToElement(text[..MaxEchoedLength]) : element;
    }
}
