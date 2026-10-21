using Domain.Handlers;
using PointSquaresService.Endpoints.Requests;

namespace PointSquaresService.Endpoints.Responses;

public record ImportPointsResponse(
    int Received,
    int Added,
    int AlreadyInCollection,
    int Excluded,
    IReadOnlyList<ImportRows.ExcludedRow> ExcludedRows,
    bool ExcludedRowsTruncated,
    PointsCollectionResponse Collection
)
{
    public static ImportPointsResponse From(ImportRows rows, ImportPointsResult result) =>
        new(
            rows.Received,
            result.Added,
            result.AlreadyInCollection,
            rows.Excluded,
            rows.ExcludedRows,
            rows.Excluded > rows.ExcludedRows.Count,
            PointsCollectionResponse.From(result.Collection)
        );
}
