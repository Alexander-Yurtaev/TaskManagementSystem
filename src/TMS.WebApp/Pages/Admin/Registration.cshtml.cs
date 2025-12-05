using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TMS.Common.Enums;
using TMS.WebApp.Services;

namespace TMS.WebApp.Pages.Admin;

public class RegistrationModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<RegistrationModel> _logger;

    public RegistrationModel(IAuthService authService, ILogger<RegistrationModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public UserRole Role { get; set; } = UserRole.User;

    public List<UserRole> AvailableRoles { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public IActionResult OnGet(string? returnUrl = null)
    {
        // Проверяем, что пользователь аутентифицирован
        if (!_authService.IsAuthenticated())
        {
            return RedirectToPage("/Admin/Login", new { returnUrl = "/Admin/Registration" });
        }

        // Получаем роль текущего пользователя из токена
        var currentUserRole = GetCurrentUserRole();
        _logger.LogDebug("currentUserRole: {CurrentUserRole}", currentUserRole);
        // Проверяем права доступа
        if (currentUserRole != UserRole.Admin && currentUserRole != UserRole.SuperAdmin)
        {
            ErrorMessage = "Access denied. Only Admin or SuperAdmin can access this page.";
            return Page();
        }

        // Настраиваем доступные роли в зависимости от текущей роли пользователя
        SetupAvailableRoles(currentUserRole);

        ReturnUrl = returnUrl ?? "/Admin/Login";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Проверяем аутентификацию и права доступа
        if (!_authService.IsAuthenticated())
        {
            return RedirectToPage("/Admin/Login");
        }

        var currentUserRole = GetCurrentUserRole();
        
        if (!(new[] { UserRole.Admin, UserRole.SuperAdmin }.Contains(currentUserRole)))
        {
            ErrorMessage = "Access denied. Only Admin or SuperAdmin can register users.";
            return Page();
        }

        // Настраиваем доступные роли для формы
        SetupAvailableRoles(currentUserRole);

        // Проверка входных данных
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Email))
        {
            ErrorMessage = "Please enter username, password, and email";
            return Page();
        }

        // Проверка, что выбранная роль доступна текущему пользователю
        if (!AvailableRoles.Contains(Role))
        {
            ErrorMessage = "Selected role is not allowed for your permission level";
            return Page();
        }

        // TODO: Здесь нужно вызвать метод регистрации, а не логина
        // Временная заглушка - вместо LoginAsync должен быть RegisterAsync
        var success = await _authService.RegistrationAsync(Username, Email, Password, Role);

        if (success)
        {
            return RedirectToPage(ReturnUrl);
        }

        ErrorMessage = "Invalid registration attempt";
        return Page();
    }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; } = "/Admin/Login";

    private UserRole GetCurrentUserRole()
    {
        var role = _authService.GetCurrentUserRole();
        return role ?? UserRole.User; // Возвращаем User как роль по умолчанию если не удалось определить
    }

    private void SetupAvailableRoles(UserRole currentUserRole)
    {
        if (currentUserRole == UserRole.SuperAdmin)
        {
            AvailableRoles = new List<UserRole> { UserRole.Admin };
        }
        else if (currentUserRole == UserRole.Admin)
        {
            AvailableRoles = new List<UserRole> { UserRole.User, UserRole.Admin };
        }
        else
        {
            AvailableRoles = new List<UserRole>();
        }
    }
}