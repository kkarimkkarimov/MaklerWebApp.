using MaklerWebApp.MVC.Models;
using MaklerWebApp.MVC.Services.Api;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MaklerWebApp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMaklerApiClient _maklerApiClient;

        public HomeController(IMaklerApiClient maklerApiClient)
        {
            _maklerApiClient = maklerApiClient;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ApiHealthy"] = await _maklerApiClient.IsHealthyAsync(HttpContext.RequestAborted);
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
