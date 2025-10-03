using Microsoft.AspNetCore.Mvc;

namespace SmartLMS.Web.Areas.Admin.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
