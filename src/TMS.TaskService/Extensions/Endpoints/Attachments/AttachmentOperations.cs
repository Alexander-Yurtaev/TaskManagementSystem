namespace TMS.TaskService.Extensions.Endpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class AttachmentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddAttachmentsOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.AddCreateAttachmentOperations();
        endpoints.AddReadAttachmentOperations();
    }
}