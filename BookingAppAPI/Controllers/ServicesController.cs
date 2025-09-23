using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;

namespace BookingAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Services
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Services>>> GetServices()
        {
            return await _context.Services.ToListAsync();
        }
        [Authorize]
        [HttpGet]
        [Route("api/services/GetAllServices")]
        public async Task<ActionResult<IEnumerable<Services>>> GetAllServices()
        {
            try
            {
                var services = await _context.Services
                    .Include(s => s.Subtopics)
                    .ThenInclude(st => st.Bulletins)
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // GET: api/Services/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Services>> GetServices(int id)
        {
            var services = await _context.Services.FindAsync(id);

            if (services == null)
            {
                return NotFound();
            }

            return services;
        }

        // PUT: api/Services/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServices(int id, Services services)
        {
            if (id != services.UniqueId)
            {
                return BadRequest();
            }

            _context.Entry(services).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Services>> PostServices(Services services)
        {
            _context.Services.Add(services);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServices", new { id = services.UniqueId }, services);
        }
        [HttpGet("GetUserByEmail")]
        public async Task<ActionResult<object>> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var user = await _context.AppUsers
                .Where(u => u.LoginEmail.ToLower() == email.ToLower())
                .Select(u => new
                {
                    u.UniqueId,
                    u.FullName,
                    u.LoginEmail,
                    u.PhoneNumber,
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }
        [HttpGet("GetUserById")]
        public async Task<ActionResult<object>> GetUserById(int uniqueId)
        {
            if (uniqueId == null)
                return BadRequest("UniqueId is required.");

            var user = await _context.AppUsers
                .Where(u => u.UniqueId == uniqueId)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServices(int id)
        {
            var services = await _context.Services.FindAsync(id);
            if (services == null)
            {
                return NotFound();
            }

            _context.Services.Remove(services);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.LoginEmail == email);
            if (user == null)
                return NotFound("User not found.");

            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            // Save token & expiry to database
            user.ResetToken = token;
            user.ResetTokenExpiry = expiry;
            await _context.SaveChangesAsync();

            string resetLink = $"http://appointment.bitprosofttech.com/UserAccount/ResetPassword?token={token}&email={email}";

            await SendResetEmail(email, resetLink);

            return Ok("Password reset email sent successfully.");
        }
        private async Task SendResetEmail(string recipientEmail, string resetLink)
        {
            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential("askrashid04@gmail.com", "kxgl wgwb zjkq elhv");
                smtpClient.EnableSsl = true;

                var message = new MailMessage("askrashid04@gmail.com", recipientEmail)
                {
                    Subject = "Reset Your Password - Ask Rashid",
                    IsBodyHtml = true,  
                    Body = $@"
<!DOCTYPE html>
<html>
<head>
  <style>
    body {{
        font-family: Arial, sans-serif;
        background-color: #f4f4f4;
        padding: 20px;
    }}
    .container {{
        max-width: 500px;
        margin: 0 auto;
        background: #fff;
        border-radius: 8px;
        padding: 30px;
        box-shadow: 0 4px 8px rgba(0,0,0,0.1);
        text-align: center;
    }}
    .logo {{
        width: 120px;
        margin-bottom: 20px;
    }}
    h2 {{
        color: #333;
    }}
    p {{
        color: #555;
        font-size: 14px;
        line-height: 1.6;
    }}
    .button {{
        display: inline-block;
        margin-top: 20px;
        padding: 12px 24px;
        background-color: #007BFF;
        color: #fff;
        font-size: 16px;
        font-weight: bold;
        text-decoration: none;
        border-radius: 6px;
    }}
    .button:hover {{
        background-color: #0056b3;
    }}
    .footer {{
        margin-top: 20px;
        font-size: 12px;
        color: #999;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <h2>Password Reset Request</h2>
    <p>Hello,</p>
    <p>We received a request to reset your password for your Ask Rashid account.</p>
    <p>Click the button below to reset your password. This link is valid for <b>5 minutes</b>.</p>
    <a href='{resetLink}' class='button'>Reset Password</a>
    <div class='footer'>
      <p>If you did not request a password reset, please ignore this email.</p>
      <p>© {DateTime.Now.Year} Ask Rashid. All rights reserved.</p>
    </div>
  </div>
</body>
</html>"
                };

                try
                {
                    await smtpClient.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private bool ServicesExists(int id)
        {
            return _context.Services.Any(e => e.UniqueId == id);
        }
    }
}
