using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Si el usuario ya está autenticado, redirigir al dashboard apropiado
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Funcionario"))
                {
                    return RedirectToAction("Dashboard", "Funcionario");
                }
                else if (User.IsInRole("Aprendiz"))
                {
                    return RedirectToAction("Dashboard", "Aprendiz");
                }
            }
            
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
