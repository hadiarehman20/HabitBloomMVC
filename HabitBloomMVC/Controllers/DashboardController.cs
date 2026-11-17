using Microsoft.AspNetCore.Mvc;

namespace HabitBloomMVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return Content("DASHBOARD CONTROLLER IS WORKING!");
        }
    }
}