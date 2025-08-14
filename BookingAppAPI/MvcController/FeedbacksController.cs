using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using BookingAppAPI.DB.Models;

namespace BookingAppAPI.Controllers
{
    public class feedbacksController : Controller
    {
        private readonly AppDbContext _context;

        public feedbacksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Feedbacks
        public async Task<IActionResult> Index(string category, string search, string dateFilter)
        {
            var feedbacks = _context.Feedbacks.AsQueryable();

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                feedbacks = feedbacks.Where(f => f.Category == category);
            }

            // Filter by search
            if (!string.IsNullOrEmpty(search))
            {
                feedbacks = feedbacks.Where(f =>
                    f.Name.Contains(search) ||
                    f.Email.Contains(search) ||
                    f.Message.Contains(search));
            }

            // Filter by date
            if (!string.IsNullOrEmpty(dateFilter))
            {
                var today = DateTime.UtcNow.Date;
                switch (dateFilter)
                {
                    case "today":
                        feedbacks = feedbacks.Where(f => f.CreatedDate.Date == today);
                        break;
                    case "thisWeek":
                        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                        feedbacks = feedbacks.Where(f => f.CreatedDate.Date >= startOfWeek);
                        break;
                    case "thisMonth":
                        feedbacks = feedbacks.Where(f => f.CreatedDate.Month == today.Month && f.CreatedDate.Year == today.Year);
                        break;
                    case "thisYear":
                        feedbacks = feedbacks.Where(f => f.CreatedDate.Year == today.Year);
                        break;
                }
            }

            // Pass categories to view
            ViewBag.Categories = Enum.GetNames(typeof(ContactCategory)).ToList();
            ViewBag.SelectedCategory = category ?? "";
            ViewBag.SelectedDateFilter = dateFilter ?? "";

            var list = await feedbacks.OrderByDescending(f => f.CreatedDate).ToListAsync();
            return View(list);
        }
    }
}
