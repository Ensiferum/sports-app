using Microsoft.AspNetCore.Http.HttpResults;
using SportsAggregator.Domain;
using SportsAggregator.Api.Services;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Domain.Results;

namespace SportsAggregator.Api.Endpoints;

public static class GameEndpoints
{
    public static RouteGroupBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty);

        group.MapGet(
                "/games",
                async Task<Results<Ok<IReadOnlyList<GameResponse>>, ValidationProblem, ProblemHttpResult>> (
                    [AsParameters] GetGamesRequest request,
                    GameQueryService queryService,
                    CancellationToken cancellationToken) =>
                {
                    var errors = Validate(request);
                    if (errors.Count > 0)
                    {
                        return TypedResults.ValidationProblem(errors);
                    }

                    var result = await queryService.GetGamesAsync(request, cancellationToken);
                    if (result.IsSuccess)
                    {
                        return TypedResults.Ok(result.Value);
                    }

                    var error = result.Error;
                    return TypedResults.Problem(
                        statusCode: MapToStatusCode(error.Type),
                        title: "Unable to retrieve games",
                        detail: error.Message,
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = error.Code
                        });
                })
            .WithName("GetGames");

        return group;
    }

    private static Dictionary<string, string[]> Validate(GetGamesRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (request.From.HasValue && request.To.HasValue && request.From.Value > request.To.Value)
        {
            errors[nameof(request.From)] = ["From must be less than or equal to To."];
        }

        if (!string.IsNullOrWhiteSpace(request.Sport) && !SportTypes.IsValid(request.Sport))
        {
            errors[nameof(request.Sport)] = ["Sport is not valid."];
        }

        return errors;
    }

    private static int MapToStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
