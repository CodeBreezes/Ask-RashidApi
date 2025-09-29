using BookingAppAPI.DB;
using BookingAppAPI.DB.Models.User;
using BookingAppAPI.ViewModels;
using Bpst.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;


namespace BookingAppAPI.MvcController
{

    public class UserAccountController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public UserAccountController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
      
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/UserAccount/Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.LoginEmail == model.LoginName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View(model);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UniqueId.ToString()),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(ClaimTypes.Email, user.LoginEmail ?? string.Empty)
    };

            if (user.Roles != null && user.Roles.Any())
            {
                foreach (var role in user.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

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
                string email = "askrashid04@gmail.com";

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
