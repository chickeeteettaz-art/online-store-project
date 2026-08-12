using Microsoft.AspNetCore.Mvc;

namespace online_store_project.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
