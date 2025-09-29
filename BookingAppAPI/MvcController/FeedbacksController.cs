using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using BookingAppAPI.DB.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookingAppAPI.ViewModels;

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

            // UAE time zone
            var uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            var nowUae = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, uaeTimeZone);

            // Compute UTC range for filtering (SQL-translatable)
            DateTime startUtc;
            DateTime endUtc;

            if (!string.IsNullOrEmpty(dateFilter))
            {
                switch (dateFilter)
                {
                    case "today":
                        startUtc = TimeZoneInfo.ConvertTimeToUtc(nowUae.Date, uaeTimeZone);
                        endUtc = TimeZoneInfo.ConvertTimeToUtc(nowUae.Date.AddDays(1), uaeTimeZone);
                        feedbacks = feedbacks.Where(f => f.CreatedDate >= startUtc && f.CreatedDate < endUtc);
                        break;

                    case "thisWeek":
                        var startOfWeek = nowUae.Date.AddDays(-(int)nowUae.DayOfWeek);
                        startUtc = TimeZoneInfo.ConvertTimeToUtc(startOfWeek, uaeTimeZone);
                        endUtc = TimeZoneInfo.ConvertTimeToUtc(startOfWeek.AddDays(7), uaeTimeZone);
                        feedbacks = feedbacks.Where(f => f.CreatedDate >= startUtc && f.CreatedDate < endUtc);
                        break;

                    case "thisMonth":
                        var startOfMonth = new DateTime(nowUae.Year, nowUae.Month, 1);
                        var startOfNextMonth = startOfMonth.AddMonths(1);
                        startUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonth, uaeTimeZone);
                        endUtc = TimeZoneInfo.ConvertTimeToUtc(startOfNextMonth, uaeTimeZone);
                        feedbacks = feedbacks.Where(f => f.CreatedDate >= startUtc && f.CreatedDate < endUtc);
                        break;

                    case "thisYear":
                        var startOfYear = new DateTime(nowUae.Year, 1, 1);
                        var startOfNextYear = startOfYear.AddYears(1);
                        startUtc = TimeZoneInfo.ConvertTimeToUtc(startOfYear, uaeTimeZone);
                        endUtc = TimeZoneInfo.ConvertTimeToUtc(startOfNextYear, uaeTimeZone);
                        feedbacks = feedbacks.Where(f => f.CreatedDate >= startUtc && f.CreatedDate < endUtc);
                        break;
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


        public async Task<IActionResult> FeedbackDetail(int id)
        {
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null) return NotFound();

            // Try fetch user from AppUsers if exists
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.UniqueId == feedback.userId);

            var feedbackVM = new FeedbackViewModel
            {
                Id = feedback.Id,
                Name = feedback.Name ?? "No Name",
                Email = feedback.Email ?? "No Email",
                Category = feedback.Category ?? "No Category",
                Message = feedback.Message ?? "No Message",
                CreatedDate = feedback.CreatedDate,

                UserId = user?.UniqueId,
                UserUniqueId = user?.UniqueId,
                FullName = user?.FullName ?? "No Name",
                UserEmail = user?.Email,
                UserPhoneNumber = user?.PhoneNumber,
                ProfileImageUrl = user?.ProfileImageUrl,
                Address = user?.Address,
                DateOfBirth = user?.DateOfBirth,
                Gender = user?.Gender
            };

            return View(feedbackVM);
        }
    }

    }
