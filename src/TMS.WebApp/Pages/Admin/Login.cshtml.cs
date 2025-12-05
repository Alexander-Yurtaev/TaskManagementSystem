using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TMS.WebApp.Services;

namespace TMS.WebApp.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/Index";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Please enter username and password";
            return Page();
        }

        var success = await _authService.LoginAsync(Username, Password);

        if (success)
        {
            return RedirectToPage(ReturnUrl);
        }

        ErrorMessage = "Invalid login attempt";
        return Page();
    }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; } = "/Index";
}