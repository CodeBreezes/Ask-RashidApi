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
