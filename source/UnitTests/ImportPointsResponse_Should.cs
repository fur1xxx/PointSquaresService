using System.Text.Json;
using Domain.Handlers;
using Persistence.Core.Models;
using PointSquaresService.Endpoints.Requests;
using PointSquaresService.Endpoints.Responses;
using Xunit;

namespace UnitTests;

public class ImportPointsResponse_Should
{
    [Fact]
    public void Serialize_ExcludedRowWithoutX_LeavesXOut()
    {
        var timestamp = new DateTimeOffset(2026, 10, 21, 9, 15, 2, 123, 456, TimeSpan.Zero);
        var collection = new PointsCollectionReadModel(7, "Survey", 1, 0, timestamp, timestamp);
        var rows = ImportRowsReader.Read(JsonSerializer.Deserialize<JsonElement>("""[{"x":1,"y":2},7]"""));

        var json = JsonSerializer.Serialize(ImportPointsResponse.From(rows, new ImportPointsResult(1, 0, collection)), JsonSerializerOptions.Web);

        Assert.Equal(
            """{"received":2,"added":1,"alreadyInCollection":0,"excluded":1,"excludedRows":[{"row":2,"reason":"Row must be an object with x and y"}],"excludedRowsTruncated":false,"collection":{"id":7,"name":"Survey","pointsCount":1,"squaresCount":0,"createdAt":"2026-10-21T09:15:02.123456+00:00","updatedAt":"2026-10-21T09:15:02.123456+00:00"}}""",
            json
        );
    }
}
