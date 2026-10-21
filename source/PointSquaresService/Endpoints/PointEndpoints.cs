using Domain.Core;
using Domain.Handlers;
using Microsoft.AspNetCore.Mvc;
using PointSquaresService.Endpoints.Requests;
using PointSquaresService.Endpoints.Responses;
using PointSquaresService.Validators;

namespace PointSquaresService.Endpoints;

public static class PointEndpoints
{
    public static void AddPointEndpoints(this RouteGroupBuilder app)
    {
        app.MapPost("public/v1/collections/{id:long}/points", AddPoint)
            .AddEndpointFilter<ValidateModel<PointRequest>>()
            .Produces<PointsCollectionResponse>(StatusCodes.Status201Created);

        app.MapDelete("public/v1/collections/{id:long}/points", RemovePoint)
            .AddEndpointFilter<ValidateModel<PointRequest>>()
            .Produces<PointsCollectionResponse>();

        app.MapPost("public/v1/collections/{id:long}/imports", ImportPoints)
            .AddEndpointFilter<ValidateModel<ImportPointsRequest>>()
            .Produces<ImportPointsResponse>();
    }

    private static async Task<IResult> AddPoint(
        [FromRoute] long id,
        [FromBody] PointRequest request,
        [FromServices] AddPointHandler handler,
        CancellationToken cancellationToken
    )
    {
        var collection = await handler.HandleAsync(id, new Point(request.X!.Value, request.Y!.Value), cancellationToken);

        return Results.Json(PointsCollectionResponse.From(collection), statusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> RemovePoint(
        [FromRoute] long id,
        [FromBody] PointRequest request,
        [FromServices] RemovePointHandler handler,
        CancellationToken cancellationToken
    )
    {
        var collection = await handler.HandleAsync(id, new Point(request.X!.Value, request.Y!.Value), cancellationToken);

        return Results.Ok(PointsCollectionResponse.From(collection));
    }

    private static async Task<IResult> ImportPoints(
        [FromRoute] long id,
        [FromBody] ImportPointsRequest request,
        [FromServices] ImportPointsHandler handler,
        CancellationToken cancellationToken
    )
    {
        var rows = ImportRowsReader.Read(request.Points);
        var result = await handler.HandleAsync(id, rows.Points, cancellationToken);

        return Results.Ok(ImportPointsResponse.From(rows, result));
    }
}
