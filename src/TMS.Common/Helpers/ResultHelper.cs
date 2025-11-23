using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TMS.Common.Helpers;

public static class ResultHelper
{
    public static IResult CreateProblemResult(string? detail, int statusCode, ILogger logger, Exception? ex = null)
    {
        logger.LogError(ex, "Operation failed: {Detail}", detail);
        return Results.Problem(detail: "Internal server error", statusCode: statusCode);
    }

    public static IResult CreateInternalServerErrorProblemResult(ILogger logger, Exception? ex = null)
    {
        return CreateProblemResult("Internal server error", StatusCodes.Status500InternalServerError, logger, ex);
    }

    public static IResult CreateValidationErrorResult(
            string entityName,
            string entityIdentifier,
            string? errorMessage,
            ILogger logger)
    {
        logger.LogWarning("{EntityName} validation failed: {EntityIdentifier}, Error: {Error}", entityName, entityIdentifier, errorMessage);
        return Results.Problem(detail: errorMessage, statusCode: StatusCodes.Status400BadRequest);
    }
}
