namespace TMS.TaskService.Extensions.Endpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class CommentsOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddCommentsOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.AddCreateCommentOperations();
        endpoints.AddReadCommentOperations();
    }
}