using Common.ErrorCodes;
using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Persistence.Core.Repositories;
using PointSquaresService.Endpoints.Requests;
using PointSquaresService.Endpoints.Responses;
using PointSquaresService.Validators;

namespace PointSquaresService.Endpoints;

public static class PointsCollectionEndpoints
{
    public static void AddPointsCollectionEndpoints(this RouteGroupBuilder app)
    {
        app.MapPost("public/v1/collections", CreatePointsCollection)
            .AddEndpointFilter<ValidateModel<CreatePointsCollectionRequest>>()
            .Produces<PointsCollectionResponse>(StatusCodes.Status201Created);

        app.MapGet("public/v1/collections", GetPointsCollections)
            .AddEndpointFilter<ValidateModel<GetPointsCollectionsRequest>>()
            .Produces<GetPointsCollectionsResponse>();

        app.MapGet("public/v1/collections/{id:long}", GetPointsCollection).Produces<PointsCollectionResponse>();
    }

    private static async Task<IResult> CreatePointsCollection(
        [FromBody] CreatePointsCollectionRequest request,
        [FromServices] PointsCollectionRepository repository,
        CancellationToken cancellationToken
    )
    {
        var collection = await repository.CreateAsync(request.Name!.Trim(), cancellationToken);

        return Results.Created($"/point-squares/public/v1/collections/{collection.Id}", PointsCollectionResponse.From(collection));
    }

    private static async Task<IResult> GetPointsCollections(
        [AsParameters] GetPointsCollectionsRequest request,
        [FromServices] PointsCollectionRepository repository,
        CancellationToken cancellationToken
    )
    {
        var collections = await repository.ListAsync(request.Limit, cancellationToken);

        return Results.Ok(GetPointsCollectionsResponse.From(collections));
    }

    private static async Task<IResult> GetPointsCollection(
        [FromRoute] long id,
        [FromServices] PointsCollectionRepository repository,
        CancellationToken cancellationToken
    )
    {
        var collection =
            await repository.GetAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Collection {id} does not exist.", ResponseErrorCodes.CollectionNotFound);

        return Results.Ok(PointsCollectionResponse.From(collection));
    }
}
