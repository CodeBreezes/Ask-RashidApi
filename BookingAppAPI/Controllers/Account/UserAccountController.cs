using Microsoft.AspNetCore.Mvc;
 using BookingAppAPI.DB; 
using Bpst.API.Services.UserAccount;
using Bpst.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Bpst.API.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountController(AppDbContext context, IUserAccountService userService) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IUserAccountService _userService = userService;

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
        [HttpPost("CheckEmailExists")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            bool exists = await _context.AppUsers
                .AnyAsync(u => u.LoginEmail.ToLower() == email.ToLower());

            return exists;
        }

    }
}
