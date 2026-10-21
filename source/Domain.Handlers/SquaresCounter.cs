using Domain.Core;
using Persistence.Core.Repositories;

namespace Domain.Handlers;

public class SquaresCounter(PointsRepository pointsRepository)
{
    public async Task<long> CountSquaresWithCornerAsync(long collectionId, Point point, CancellationToken cancellationToken)
    {
        var columnAndRow = await pointsRepository.GetColumnAndRowAsync(collectionId, point, cancellationToken);
        var oppositeCorners = new SquareFinder(columnAndRow.Append(point)).FindCornersThatCompleteSquares(point);
        return await pointsRepository.CountExistingAsync(collectionId, oppositeCorners, cancellationToken);
    }
}
