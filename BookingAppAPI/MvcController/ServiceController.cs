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

        // GET: Service/Create

        // GET: Create or Edit Service
        public async Task<IActionResult> Create(int id = 0)
        {
            Services service = null;

            if (id != 0)
            {
                // Try to load existing service (for update)
                service = await _context.Services
                    .Include(s => s.Subtopics)
                        .ThenInclude(st => st.Bulletins)
                    .FirstOrDefaultAsync(s => s.UniqueId == id);
            }

            if (service == null)
            {
                // Create new default service if not found
                service = new Services
                {
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
                            CreatedDate = DateTime.Now,
                            LastUpdatedDate = DateTime.Now
                        }
                    }
                }
            }
                };
            }

            return View(service);
        }

        // POST: Create or Update Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Services services)
        {
            if (ModelState.IsValid)
            {
                if (services.UniqueId == 0)
                {
                    // New service
                    _context.Add(services);
                }
                else
                {
                    // Update existing service
                    _context.Update(services);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(services);
        }

        // GET: Service/Edit/5



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




        private bool ServicesExists(int id)
        {
            return _context.Services.Any(e => e.UniqueId == id);
        }
    }
}
