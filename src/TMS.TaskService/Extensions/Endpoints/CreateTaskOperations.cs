using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Extensions.Endpoints;

/// <summary>
/// 
/// </summary>
public static class CreateTaskOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddCreateTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        AddCreateTaskOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddCreateTaskOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/task", (
            [FromServices] TaskDataContext db,
            [FromBody] TaskEntity task) =>
        {
            var isExist = db.Tasks.Any(t => t.Title == task.Title);
            if (isExist)
            {
                Results.BadRequest($"Задача с Title={task.Title} уже существует.");
            }


        });
    }
}