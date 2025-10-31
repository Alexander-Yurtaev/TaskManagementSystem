using Microsoft.AspNetCore.Mvc;

namespace TMS.ApiGateway.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request is not null)
            {
                ViewBag.Scheme = request.Scheme;
                ViewBag.Host = request.Host;
            }

            return View();
        }
    }
}
