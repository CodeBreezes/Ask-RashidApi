using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using BookingAppAPI.DB.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingAppAPI.Controllers
{
    public class feedbacksController : Controller
    {
        private readonly AppDbContext _context;

        public feedbacksController(AppDbContext context)
        {
            _context = context;
        }

   
        public IActionResult Index(string dateFilter, string category)
        {
            var feedbacks = _context.Feedbacks.AsQueryable();

            // Apply date filter
            var uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            var nowUae = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, uaeTimeZone);

            if (!string.IsNullOrEmpty(dateFilter))
            {
                if (dateFilter == "today")
                {
                    feedbacks = feedbacks.Where(f =>
                        TimeZoneInfo.ConvertTimeFromUtc(f.CreatedDate, uaeTimeZone).Date == nowUae.Date);
                }
                else if (dateFilter == "thisWeek")
                {
                    var startOfWeek = nowUae.AddDays(-(int)nowUae.DayOfWeek);
                    feedbacks = feedbacks.Where(f =>
                        TimeZoneInfo.ConvertTimeFromUtc(f.CreatedDate, uaeTimeZone) >= startOfWeek);
                }
                else if (dateFilter == "thisMonth")
                {
                    feedbacks = feedbacks.Where(f =>
                        TimeZoneInfo.ConvertTimeFromUtc(f.CreatedDate, uaeTimeZone).Month == nowUae.Month &&
                        TimeZoneInfo.ConvertTimeFromUtc(f.CreatedDate, uaeTimeZone).Year == nowUae.Year);
                }
                else if (dateFilter == "thisYear")
                {
                    feedbacks = feedbacks.Where(f =>
                        TimeZoneInfo.ConvertTimeFromUtc(f.CreatedDate, uaeTimeZone).Year == nowUae.Year);
                }
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                feedbacks = feedbacks.Where(f => f.Category == category);
            }

            // Date filter dropdown
            ViewBag.DateFilterOptions = new SelectList(new[]
            {
            new { Value = "today", Text = "Today" },
            new { Value = "thisWeek", Text = "This Week" },
            new { Value = "thisMonth", Text = "This Month" },
            new { Value = "thisYear", Text = "This Year" }
        }, "Value", "Text", dateFilter);

            // Category filter dropdown
            var categories = _context.Feedbacks
                .Select(f => f.Category)
                .Distinct()
                .ToList();
            ViewBag.CategoryOptions = new SelectList(categories, category);

            return View(feedbacks.ToList());
        }
    }

}
