// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalysis.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public HomeController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index() => View();

        public IActionResult About()
        {
            var licensePath = Path.Combine(_environment.ContentRootPath, "LICENSE");
            var licenseText = System.IO.File.Exists(licensePath)
                ? System.IO.File.ReadAllText(licensePath)
                : "License file not found.";

            ViewBag.LicenseText = licenseText;
            return View();
        }
    }
}
