using Common.Constants;
using Common.ErrorCodes;
using Common.Exceptions;
using Domain.Core;
using Persistence.Core;
using Persistence.Core.Repositories;

namespace Domain.Handlers;

public class ImportPointsHandler(UnitOfWork unitOfWork, PointsCollectionRepository pointsCollectionRepository, PointsRepository pointsRepository)
{
    public async Task<ImportPointsResult> HandleAsync(long collectionId, IReadOnlyList<Point> points, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (!await pointsCollectionRepository.LockAsync(collectionId, cancellationToken))
            throw new NotFoundException($"Collection {collectionId} does not exist.", ResponseErrorCodes.CollectionNotFound);

        var collection = await pointsCollectionRepository.GetAsync(collectionId, cancellationToken);
        var storedPoints = (await pointsRepository.GetAllAsync(collectionId, cancellationToken)).ToHashSet();
        var newPoints = points.Where(point => !storedPoints.Contains(point)).ToHashSet();
        if (collection!.PointsCount + newPoints.Count > PointsCollectionConstants.MaxPointsCount)
            throw new DomainException(
                $"Collection {collectionId} has {collection.PointsCount} points; {newPoints.Count} new points would pass the limit of {PointsCollectionConstants.MaxPointsCount}.",
                ResponseErrorCodes.CollectionFull
            );

        if (newPoints.Count > 0)
        {
            var squares = new SquareFinder(storedPoints.Concat(newPoints)).CountSquaresWithAnyCornerIn(newPoints, cancellationToken);
            await pointsRepository.AddRangeAsync(collectionId, newPoints, cancellationToken);
            await pointsCollectionRepository.ChangeCountsAsync(
                collectionId,
                pointsChange: newPoints.Count,
                squaresChange: squares,
                cancellationToken
            );
        }

        var changed = await pointsCollectionRepository.GetAsync(collectionId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new ImportPointsResult(newPoints.Count, points.Count(storedPoints.Contains), changed!);
    }
}
