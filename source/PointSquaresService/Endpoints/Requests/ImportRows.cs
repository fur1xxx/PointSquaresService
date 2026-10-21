using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Core;

namespace PointSquaresService.Endpoints.Requests;

public record ImportRows(int Received, IReadOnlyList<Point> Points, int Excluded, IReadOnlyList<ImportRows.ExcludedRow> ExcludedRows)
{
    public record ExcludedRow(
        int Row,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonElement? X,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonElement? Y,
        string Reason
    );
}
