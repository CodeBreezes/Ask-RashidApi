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
using Stripe;

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
        [HttpPost("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.LoginEmail == email);
            if (user == null)
                return NotFound("User not found.");

            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            user.ResetToken = token;
            user.ResetTokenExpiry = expiry;
            await _context.SaveChangesAsync();

            string resetLink = $"http://appointment.bitprosofttech.com/UserAccount/DeleteAccount?token={token}&email={email}";

            await SendDeleteAccountEmail(email, resetLink);

            return Ok("Email sent successfully.");
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
                    Body = $"Click the link below to reset your password (valid for 5 minutes):\n{resetLink}"

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
        private async Task SendDeleteAccountEmail(string recipientEmail, string deleteLink)
        {
            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential("askrashid04@gmail.com", "kxgl wgwb zjkq elhv");  
                smtpClient.EnableSsl = true;

                var message = new MailMessage("askrashid04@gmail.com", recipientEmail)
                {
                    Subject = "Confirm Account Deletion - Ask Rashid",
                    Body = $"Click the link below to Delete your Account (valid for 5 minutes):\n{deleteLink}"

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
