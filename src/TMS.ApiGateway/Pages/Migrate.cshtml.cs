using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TMS.ApiGateway.Services;
using TMS.ApiGateway.Models;

[IgnoreAntiforgeryToken]
public class MigrateModel : PageModel
{
    private readonly IMigrationService _migrationService;

    [BindProperty]
    public List<MigrationResult> MigrationResults { get; set; } = new();

    public MigrateModel(IMigrationService migrationService)
    {
        _migrationService = migrationService;
    }

    // Этот метод вызывается при GET запросе на /Migrate
    public void OnGet()
    {
        // При первом заходе просто показываем страницу
    }

    // Этот метод вызывается при POST запросе на /Migrate
    // (когда нажимаем кнопку в форме)
    public async Task<IActionResult> OnPostAsync()
    {
        MigrationResults = await _migrationService.MigrateAllAsync();
        return Page(); // Возвращаем ту же страницу с результатами
    }

    // Для AJAX запросов
    public async Task<IActionResult> OnPostMigrateServiceAsync(string serviceName)
    {
        var result = await _migrationService.MigrateServiceAsync(serviceName);
        return new JsonResult(result);
    }
}