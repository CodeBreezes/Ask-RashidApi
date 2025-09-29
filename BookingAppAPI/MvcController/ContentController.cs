using Microsoft.AspNetCore.Mvc;
using BookingAppAPI.DB;
using BookingAppAPI.DB.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookingAppAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ContentController : Controller
    {
        private readonly AppDbContext _context;

        public ContentController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Services()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        public IActionResult CreateService()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateService(Services model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = model.LastUpdatedDate = DateTime.Now;

                _context.Services.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Services");
            }

            return View(model);
        }


        // ------------------ SUBTOPICS ------------------

        public IActionResult ManageSubtopics(int serviceId)
        {
            var service = _context.Services.FirstOrDefault(s => s.UniqueId == serviceId);
            if (service == null) return NotFound();

            var subtopics = _context.Subtopics
                                    .Where(s => s.ServiceId == serviceId)
                                    .ToList();

            ViewBag.ServiceId = serviceId;
            ViewBag.ServiceName = service.Name;
            return View(subtopics);
        }

        public IActionResult CreateSubtopic(int serviceId)
        {
            ViewBag.ServiceId = serviceId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubtopic(Subtopics model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = model.LastUpdatedDate = DateTime.Now;
                _context.Subtopics.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageSubtopics", new { serviceId = model.ServiceId });
            }
            ViewBag.ServiceId = model.ServiceId;
            return View(model);
        }
         
        public IActionResult ManageBulletins(int subtopicId)
        {
            var subtopic = _context.Subtopics.FirstOrDefault(s => s.Id == subtopicId);
            if (subtopic == null) return NotFound();

            var bulletins = _context.Bulletins
                                    .Where(b => b.SubtopicId == subtopicId)
                                    .OrderBy(b => b.OrderIndex)
                                    .ToList();

            ViewBag.SubtopicId = subtopicId;
            ViewBag.SubtopicTitle = subtopic.Title;
            return View(bulletins);
        }

        public IActionResult CreateBulletin(int subtopicId)
        {
            ViewBag.SubtopicId = subtopicId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBulletin(Bulletins model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = model.LastUpdatedDate = DateTime.Now;
                _context.Bulletins.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageBulletins", new { subtopicId = model.SubtopicId });
            }
            ViewBag.SubtopicId = model.SubtopicId;
            return View(model);
        }
        [HttpGet]
        public IActionResult ManageService()
        {
            return View(new ServiceFullViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ManageService(ServiceFullViewModel model)
        {
            if (ModelState.IsValid)
            {
                var service = new Services
                {
                    Name = model.Name,
                    Description = model.Description,
                    Cost = model.Cost,
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsActive = true,
                    Subtopics = model.Subtopics.Select(st => new Subtopics
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

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                return RedirectToAction("Services");
            }

            return View(model);
        }

    }
}
