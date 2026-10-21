using Common.ErrorCodes;
using Common.Exceptions;
using Domain.Core;
using Persistence.Core;
using Persistence.Core.Models;
using Persistence.Core.Repositories;

namespace Domain.Handlers;

public class AddPointHandler(
    UnitOfWork unitOfWork,
    PointsCollectionRepository pointsCollectionRepository,
    PointsRepository pointsRepository,
    SquaresCounter squaresCounter
)
{
    private const int MaxPointsInCollection = 1_000_000;

    public async Task<PointsCollectionReadModel> HandleAsync(long collectionId, Point point, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (!await pointsCollectionRepository.LockAsync(collectionId, cancellationToken))
            throw new NotFoundException($"Collection {collectionId} does not exist.", ResponseErrorCodes.CollectionNotFound);

        var collection = await pointsCollectionRepository.GetAsync(collectionId, cancellationToken);
        if (await pointsRepository.ExistsAsync(collectionId, point, cancellationToken))
            throw new DomainException(
                $"Point ({point.X}, {point.Y}) is already in collection {collectionId}.",
                ResponseErrorCodes.PointAlreadyExists
            );
        if (collection!.PointsCount >= MaxPointsInCollection)
            throw new DomainException($"Collection {collectionId} already has {MaxPointsInCollection} points.", ResponseErrorCodes.CollectionFull);

        var squares = await squaresCounter.CountSquaresWithCornerAsync(collectionId, point, cancellationToken);
        await pointsRepository.AddAsync(collectionId, point, cancellationToken);
        await pointsCollectionRepository.ChangeCountsAsync(collectionId, pointsChange: 1, squaresChange: squares, cancellationToken);

        var changed = await pointsCollectionRepository.GetAsync(collectionId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return changed!;
    }
}
