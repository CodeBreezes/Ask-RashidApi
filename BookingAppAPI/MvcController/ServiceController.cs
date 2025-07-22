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

        public async Task<IActionResult> Create(int id)
        {
            var service = await _context.Services.Include(s => s.Subtopics).ThenInclude(st => st.Bulletins).FirstOrDefaultAsync(s => s.UniqueId == id);
            if (service == null)     
                service = new Services()
                {
                    Subtopics = new List<Subtopics>()
                    {

                        new Subtopics() 
                        {
                           Bulletins = new List<Bulletins>()
                           {
                               new Bulletins()
                               {
                                   Content = "Welcome to the service!",
                                   CreatedDate = DateTime.Now,
                                   LastUpdatedDate = DateTime.Now
                               }
                           },CreatedDate = DateTime.Now, LastUpdatedDate = DateTime.Now,Id = 0, Title = "General Information"
                        }
                    }
                };
            return View(service);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Services services)
        {
            if (ModelState.IsValid)
            {
                if (services.UniqueId.Equals(0))
                {
                    _context.Add(services);
                }
                else
                {
                    _context.Update(services);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(services);
        }

        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var services = await _context.Services.FindAsync(id);
            if (services == null)
            {
                return NotFound();
            }
            return View(services);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UniqueId,Name,Description,Cost,CreatedDate,LastUpdatedDate,IsActive")] Services services)
        {
            if (id != services.UniqueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(services);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicesExists(services.UniqueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(services);
        }

        public async Task<IActionResult> Delete(int? id)
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

        // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var services = await _context.Services.FindAsync(id);
            if (services != null)
            {
                _context.Services.Remove(services);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServicesExists(int id)
        {
            return _context.Services.Any(e => e.UniqueId == id);
        }
    }
}
