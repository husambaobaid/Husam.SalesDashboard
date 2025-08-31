using Microsoft.AspNetCore.Mvc;

namespace Husam.SalesDashboard.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Message = "Dashboard is live!";
            return View();
        }
    }
}