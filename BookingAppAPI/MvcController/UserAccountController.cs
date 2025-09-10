using Microsoft.AspNetCore.Mvc;

namespace BookingAppAPI.MvcController
{
    public class UserAccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Default credentials
            if (username == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString("User", username); // store in session
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid credentials!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
