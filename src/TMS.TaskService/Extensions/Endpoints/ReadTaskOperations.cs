namespace TMS.TaskService.Extensions.Endpoints;

/// <summary>
/// 
/// </summary>
public static class ReadTaskOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddReadTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        AddGetTasksOperation(endpoints);
        AddGetTaskOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetTasksOperation(IEndpointRouteBuilder endpoints)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetTaskOperation(IEndpointRouteBuilder endpoints)
    {

    }
}