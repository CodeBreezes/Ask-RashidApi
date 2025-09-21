using BookingAppAPI.DB;
using BookingAppAPI.ViewModels;
using Bpst.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAppAPI.MvcController
{
    public class UserAccountController : Controller
    {

        private readonly AppDbContext _context;

        public UserAccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return Content("Invalid link");

            //var user = await _context.AppUsers
            //    .FirstOrDefaultAsync(u =>
            //        u.LoginEmail == email &&
            //        u.ResetToken == token);

            //if (user == null)
            //    return Content("Invalid reset link.");

            //if (user.ResetTokenExpiry < DateTime.UtcNow)
            //    return Content("Reset link has expired.");

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model); 

            var result = new UpdateResponse
            {
                IsUpdated = false,
                ErrorMessages = new List<string>(),
                SuccessMessages = new List<string>()
            };

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.LoginEmail == model.Email && u.ResetToken == model.Token);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or token.");
                return View(model);
            }

            if (user.ResetTokenExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Reset link has expired.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "New password and confirm password do not match.");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Password updated successfully!";
            return View(); 
        }

    }
}
