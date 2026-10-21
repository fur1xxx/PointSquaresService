using System.Text.Json;
using Persistence.Core.Models;
using PointSquaresService.Endpoints.Responses;
using Xunit;

namespace UnitTests;

public class PointsCollectionResponse_Should
{
    [Fact]
    public void From_UtcDates_SerializesThemWithZeroOffset()
    {
        var timestamp = new DateTimeOffset(2026, 10, 21, 9, 15, 2, 123, 456, TimeSpan.Zero);
        var model = new PointsCollectionReadModel(7, "Warehouse A", 0, 0, timestamp, timestamp);

        var json = JsonSerializer.Serialize(PointsCollectionResponse.From(model), JsonSerializerOptions.Web);

        Assert.Equal(
            """{"id":7,"name":"Warehouse A","pointsCount":0,"squaresCount":0,"createdAt":"2026-10-21T09:15:02.123456+00:00","updatedAt":"2026-10-21T09:15:02.123456+00:00"}""",
            json
        );
    }
}
