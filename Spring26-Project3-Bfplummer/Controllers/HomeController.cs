using Microsoft.AspNetCore.Mvc;
using Spring26_Project3_Bfplummer.Models;
using Spring26_Project3_Bfplummer.Models;
using System.Diagnostics;

namespace Spring2026_Project3_bfplummer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

