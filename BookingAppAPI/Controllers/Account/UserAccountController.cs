using Microsoft.AspNetCore.Mvc;
 using BookingAppAPI.DB; 
using Bpst.API.Services.UserAccount;
using Bpst.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB.Models;

namespace Bpst.API.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountController(AppDbContext context, IUserAccountService userService, IWebHostEnvironment hostingEnvironment) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IUserAccountService _userService = userService;
        private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment;


        [AllowAnonymous]
        [HttpPost("UserRegistration")]
        public async Task<ActionResult<UserRegistrationResponse>> PostUser(UserRegistrationVM user)
        {
            var result = await _userService.RegisterNewUserAsync(user);
            return result;
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginVM login)
        {
            var result = await _userService.Login(login);
            return result;
        }

        [Authorize]
        [HttpPost("ChangeLoginEmail")]
        public async Task<ActionResult<UpdateResponse>> ChangeLoginEmail(string newemail, string password)
        {
            string? oldEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            // ToDo, Validate user from existing token and update his login email to new one.
            UpdateResponse result = await _userService.UpdateEmail(oldEmail, newemail, password);
            return result;
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult<UpdateResponse>> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            string? email = User.FindFirst(ClaimTypes.Email)?.Value;
            //ToDo, Validate user from existing token and update his password
            var result = await _userService.UpdatePassword(email,oldPassword, newPassword, confirmPassword);
            return result;
        }
        [HttpPost("ChangePasswordbyEmail")]
        public async Task<ActionResult<UpdateResponse>> ChangePasswordbyEmail(string email,string oldPassword, string newPassword, string confirmPassword)
        {
            var result = await _userService.UpdatePassword(email, oldPassword, newPassword, confirmPassword);
            return result;
        }
        [HttpPost("CheckEmailExists")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            bool exists = await _context.AppUsers
                .AnyAsync(u => u.LoginEmail.ToLower() == email.ToLower());

            return exists;
        }

        [HttpPost("UpdateProfile")]
        public async Task<ActionResult<UpdateResponse>> UpdateProfile([FromForm] UpdateProfileVM model)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.LoginEmail == model.Email);
            if (user == null)
                return NotFound(new UpdateResponse
                {
                    IsUpdated = false,
                    ErrorMessages = new List<string> { "User not found." }
                });

            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender;
            user.LastUpdatedDate = DateTime.UtcNow;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.Address))
                user.Address = model.Address;
            
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "UserProfiles", user.UniqueId.ToString());
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = Path.GetFileNameWithoutExtension(model.ProfileImage.FileName);
                string extension = Path.GetExtension(model.ProfileImage.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                
                user.ProfileImageUrl = $"/UserProfiles/{user.UniqueId}/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();

            return Ok(new UpdateResponse
            {
                IsUpdated = true,
                SuccessMessages = new List<string> { "Profile updated successfully." }
            });
        }

    }
}
