using BookingAppAPI.DB;
using BookingAppAPI.DB.Models.User;
using BookingAppAPI.ViewModels;
using Bpst.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;


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

        public IActionResult AdminLogin()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Login", "UserAccount");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.LoginEmail == model.LoginName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash) ||
                user.Roles == null || !user.Roles.Contains("Admin"))
            {
                ModelState.AddModelError("", "Invalid Email Id or Password");
                return View(model);
            }

            HttpContext.Session.SetString("UserEmail", user.LoginEmail);
            HttpContext.Session.SetString("UserRole", "Admin");

            return RedirectToAction("Index", "Service");
        }

        public async Task<IActionResult> DeleteAccount(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return Content("Invalid link");

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u =>
                    u.LoginEmail == email &&
                    u.ResetToken == token);

            if (user == null)
                return Content("Invalid reset link.");

            if (user.ResetTokenExpiry < DateTime.UtcNow)
                return Content("Reset link has expired.");

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        public async Task<string> CreateMasterUser()
        {
            var resultStr = string.Empty;

            try
            {
                string email = "askrashid05@gmail.com";

                var exists = await _context.AppUsers.AnyAsync(u => u.LoginEmail == email);
                if (exists)
                    return "Master user already exists.";

                var appUser = new AppUser
                {
                    LoginEmail = email,
                    Email = email,
                    FirstName = "Rashid",
                    LastName = "Bahattab",
                    PhoneNumber = "1551941751",
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("AskRashid@1974#")
                };

                _context.AppUsers.Add(appUser);
                await _context.SaveChangesAsync();

                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
                if (adminRole == null)
                {
                    adminRole = new Roles { RoleName = "Admin" };
                    _context.Roles.Add(adminRole);
                    await _context.SaveChangesAsync();
                }

                appUser.Roles = new List<string> { adminRole.RoleName };
                _context.AppUsers.Update(appUser);
                await _context.SaveChangesAsync();

                resultStr = "✅ Master User Created Successfully with Admin role.";
            }
            catch (Exception ex)
            {
                resultStr = "❌ Some Error: " + ex.Message;
            }

            return resultStr;
        }

        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return Content("Invalid link");

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u =>
                    u.LoginEmail == email &&
                    u.ResetToken == token);

            if (user == null)
                return Content("Invalid reset link.");

            if (user.ResetTokenExpiry < DateTime.UtcNow)
                return Content("Reset link has expired.");

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(ResetPasswordViewModel model)
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

            try
            {
                _context.AppUsers.Remove(user);
                await _context.SaveChangesAsync();

                result.IsUpdated = true;
                result.SuccessMessages.Add("Your account has been deleted successfully!");

                ViewBag.SuccessMessage = "Your account has been deleted successfully!";
                return View();
            }
            catch (Exception ex)
            {
                result.ErrorMessages.Add("An error occurred while deleting the account: " + ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the account.");
                return View(model);
            }
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
