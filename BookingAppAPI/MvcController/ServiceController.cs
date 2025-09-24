using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.CodeAnalysis.Host.Mef;
using BookingAppAPI.DB.Models;
using System.Net.Mail;
using System.Net;

namespace BookingAppAPI.MvcController
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Service
        public async Task<IActionResult> Index()
        {
            return View(await _context.Services.ToListAsync());
        }

        // GET: Service/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var services = await _context.Services
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (services == null)
            {
                return NotFound();
            }

            return View(services);
        }

        public async Task<IActionResult> Create(int id = 0)
        {
            Services service;

            if (id != 0)
            {
                service = await _context.Services
                    .Include(s => s.Subtopics)
                        .ThenInclude(st => st.Bulletins)
                    .FirstOrDefaultAsync(s => s.UniqueId == id);

                if (service == null)
                    return NotFound();
            }
            else
            {
                service = new Services
                {
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Subtopics = new List<Subtopics>
            {
                new Subtopics
                {
                    Title = "General Information",
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Bulletins = new List<Bulletins>
                    {
                        new Bulletins
                        {
                            Content = "Welcome to the service!",
                            OrderIndex = 1,
                            CreatedDate = DateTime.Now,
                            LastUpdatedDate = DateTime.Now
                        }
                    }
                }
            }
                };
            }

            var viewModel = new ServiceFullViewModel
            {
                UniqueId = service.UniqueId,
                Name = service.Name,
                Description = service.Description,
                Cost = service.Cost,
                Subtopics = service.Subtopics.Select(st => new SubtopicViewModel
                {
                    Title = st.Title,
                    Bulletins = st.Bulletins.Select(b => new BulletinViewModel
                    {
                        Content = b.Content,
                        OrderIndex = b.OrderIndex
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceFullViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            Services services;
            if (vm.UniqueId == 0)
            {
                services = new Services
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    Cost = vm.Cost,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Subtopics = vm.Subtopics.Select(st => new Subtopics
                    {
                        Title = st.Title,
                        CreatedDate = DateTime.Now,
                        LastUpdatedDate = DateTime.Now,
                        Bulletins = st.Bulletins.Select(b => new Bulletins
                        {
                            Content = b.Content,
                            OrderIndex = b.OrderIndex,
                            CreatedDate = DateTime.Now,
                            LastUpdatedDate = DateTime.Now
                        }).ToList()
                    }).ToList()
                };

                _context.Services.Add(services);
            }
            else
            {
                var existingService = await _context.Services
                    .Include(s => s.Subtopics)
                        .ThenInclude(st => st.Bulletins)
                    .FirstOrDefaultAsync(s => s.UniqueId == vm.UniqueId);

                if (existingService == null)
                    return NotFound();

                existingService.Name = vm.Name;
                existingService.Description = vm.Description;
                existingService.Cost = vm.Cost;
                existingService.LastUpdatedDate = DateTime.Now;

                _context.Subtopics.RemoveRange(existingService.Subtopics);

                existingService.Subtopics = vm.Subtopics.Select(st => new Subtopics
                {
                    ServiceId = existingService.UniqueId,
                    Title = st.Title,
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Bulletins = st.Bulletins.Select(b => new Bulletins
                    {
                        Content = b.Content,
                        OrderIndex = b.OrderIndex,
                        CreatedDate = DateTime.Now,
                        LastUpdatedDate = DateTime.Now
                    }).ToList()
                }).ToList();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.UniqueId == id);

            if (service == null)
                return NotFound();

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult DeleteAccount()
        {
            return View();
        }
        [HttpPost]
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
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        private bool ServicesExists(int id)
        {
            return _context.Services.Any(e => e.UniqueId == id);
        }
    }
}
