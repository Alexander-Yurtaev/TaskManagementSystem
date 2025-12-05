using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TMS.WebApp.Models;
using TMS.WebApp.Services;

namespace TMS.WebApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class MigrateModel : PageModel
{
    private readonly IMigrationService _migrationService;
    private readonly IAuthService _authService;

    public MigrateModel(IMigrationService migrationService, IAuthService authService)
    {
        _migrationService = migrationService;
        _authService = authService;
    }

    [BindProperty]
    public List<MigrationResult> MigrationResults { get; set; } = [];

    public IActionResult OnGet()
    {
        if (!_authService.IsAuthenticated())
        {
            return RedirectToPage("/Admin/Login", new { returnUrl = "/Admin/Migrate" });
        }

        MigrationResults = [];
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!_authService.IsAuthenticated())
        {
            return RedirectToPage("/Admin/login", new { returnUrl = "/Admin/Migrate" });
        }

        MigrationResults = await _migrationService.MigrateAllAsync();
        return Page();
    }
}