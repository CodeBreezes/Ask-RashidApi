// CONTROLLER: ManageController.cs
using BookingAppAPI.DB;
using BookingAppAPI.DB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAppAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageController : Controller
    {
        private readonly AppDbContext _context;

        public ManageController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult SaveService(Services model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Name is required");

            if (model.UniqueId > 0)
            {
                var existing = _context.Services.FirstOrDefault(s => s.UniqueId == model.UniqueId);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.Description = model.Description;
                    existing.Cost = model.Cost;
                    existing.LastUpdatedDate = DateTime.Now;
                }
            }
            else
            {
                model.CreatedDate = model.LastUpdatedDate = DateTime.Now;
                _context.Services.Add(model);
            }
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult SaveSubtopic(Subtopics model)
        {
            if (string.IsNullOrWhiteSpace(model.Title) || model.ServiceId == 0)
                return BadRequest("Invalid subtopic input");

            if (model.Id > 0)
            {
                var existing = _context.Subtopics.FirstOrDefault(s => s.Id == model.Id);
                if (existing != null)
                {
                    existing.Title = model.Title;
                    existing.ServiceId = model.ServiceId;
                    existing.LastUpdatedDate = DateTime.Now;
                }
            }
            else
            {
                model.CreatedDate = model.LastUpdatedDate = DateTime.Now;
                _context.Subtopics.Add(model);
            }
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult SaveBulletin(Bulletins model)
        {
            if (string.IsNullOrWhiteSpace(model.Content) || model.SubtopicId == 0)
                return BadRequest("Invalid bulletin input");

            if (model.Id > 0)
            {
                var existing = _context.Bulletins.FirstOrDefault(b => b.Id == model.Id);
                if (existing != null)
                {
                    existing.Content = model.Content;
                    existing.OrderIndex = model.OrderIndex;
                    existing.SubtopicId = model.SubtopicId;
                    existing.LastUpdatedDate = DateTime.Now;
                }
            }
            else
            {
                model.CreatedDate = model.LastUpdatedDate = DateTime.Now;
                _context.Bulletins.Add(model);
            }
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _context.Services
                        .Include(s => s.Subtopics)
                        .ThenInclude(st => st.Bulletins)
                        .ToList();
            return Json(data);
        }

        [HttpGet]
        public IActionResult GetSubtopicsByService(int serviceId)
        {
            var subs = _context.Subtopics.Where(x => x.ServiceId == serviceId).ToList();
            return Json(subs);
        }

        [HttpPost]
        public IActionResult DeleteService(int id)
        {
            var item = _context.Services.Include(s => s.Subtopics).ThenInclude(st => st.Bulletins)
                        .FirstOrDefault(s => s.UniqueId == id);
            if (item == null) return NotFound();

            _context.Bulletins.RemoveRange(item.Subtopics.SelectMany(s => s.Bulletins));
            _context.Subtopics.RemoveRange(item.Subtopics);
            _context.Services.Remove(item);
            _context.SaveChanges();

            return Ok();
        }
    }
}
