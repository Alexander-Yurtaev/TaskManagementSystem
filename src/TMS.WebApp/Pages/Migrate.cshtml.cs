using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TMS.WebApp.Models;
using TMS.WebApp.Services;

namespace TMS.WebApp.Pages;

[Authorize]
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

    public async Task<IActionResult> OnGet()
    {
        if (!_authService.IsAuthenticated())
        {
            return RedirectToPage("/Login", new { returnUrl = "/Migrate" });
        }

        MigrationResults = [];
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!_authService.IsAuthenticated())
        {
            return RedirectToPage("/Login", new { returnUrl = "/Migrate" });
        }

        MigrationResults = await _migrationService.MigrateAllAsync();
        return Page();
    }
}