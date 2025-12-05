using Microsoft.AspNetCore.Mvc.RazorPages;
using TMS.Common.Enums;
using TMS.Common.Helpers;
using TMS.WebApp.Services;

namespace TMS.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAuthService _authService;

        public IndexModel(ILogger<IndexModel> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public void OnGet()
        {
            
        }

        public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        public string? Username => User.Identity?.Name;
        public UserRole? UserRole => ClaimsHelper.GetCurrentUserRole(User);

        public bool IsAuthenticatedViaToken => _authService.IsAuthenticated();
    }
}
