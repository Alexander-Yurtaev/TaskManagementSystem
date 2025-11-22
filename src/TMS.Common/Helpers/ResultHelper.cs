using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
}
