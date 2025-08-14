using Microsoft.AspNetCore.Mvc;

namespace BookingAppAPI.MvcController
{
    public class UserAccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }



    }
}
