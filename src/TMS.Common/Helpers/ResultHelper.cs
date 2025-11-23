using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace TMS.Common.Helpers;

public static class ResultHelper
{
    public static IResult CreateProblemResult(string? detail, int statusCode, ILogger logger, Exception? ex = null)
    {
        logger.LogError(ex, "Operation failed: {Detail}", detail);
        return Results.Problem(detail: detail, statusCode: statusCode);
    }

    public static IResult CreateInternalServerErrorProblemResult(string? detail, ILogger logger, Exception? ex = null)
    {
        return CreateProblemResult(detail, StatusCodes.Status500InternalServerError, logger, ex);
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

    public static IResult CreateExternalServiceErrorResult(
            string serviceName,
            string operation,
            HttpStatusCode statusCode,
            ILogger logger,
            string? additionalInfo = null)
    {
        var detail = $"External service '{serviceName}' returned error during {operation}: {statusCode}";
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            detail += $". {additionalInfo}";
        }

        logger.LogError("External service error: {ServiceName}, Operation: {Operation}, Status: {StatusCode}",
            serviceName, operation, statusCode);

        return Results.Problem(
            detail: detail,
            statusCode: StatusCodes.Status502BadGateway);
    }
}
