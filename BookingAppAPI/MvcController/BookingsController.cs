using BookingAppAPI.DB;
using BookingAppAPI.DB;
using BookingAppAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAppAPI.MvcController
{
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Booking
        public IActionResult Index(string filter, DateTime? startDate, DateTime? endDate)
        {
            var bookings = _context.Booking
                .Include(b => b.Service)
                .AsQueryable();

            var today = DateOnly.FromDateTime(DateTime.Today);

            switch (filter)
            {
                case "today":
                    bookings = bookings.Where(b => b.StartedDate == today);
                    break;
                case "thisWeek":
                    var startOfWeek = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek));
                    bookings = bookings.Where(b => b.StartedDate >= startOfWeek && b.StartedDate <= today);
                    break;
                case "lastMonth":
                    var lastMonth = DateTime.Today.AddMonths(-1);
                    var lastMonthStart = DateOnly.FromDateTime(new DateTime(lastMonth.Year, lastMonth.Month, 1));
                    var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);
                    bookings = bookings.Where(b => b.StartedDate >= lastMonthStart && b.StartedDate <= lastMonthEnd);
                    break;
                case "thisYear":
                    var startOfYear = DateOnly.FromDateTime(new DateTime(today.Year, 1, 1));
                    bookings = bookings.Where(b => b.StartedDate >= startOfYear && b.StartedDate <= today);
                    break;
                case "custom":
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        var start = DateOnly.FromDateTime(startDate.Value);
                        var end = DateOnly.FromDateTime(endDate.Value);
                        bookings = bookings.Where(b => b.StartedDate >= start && b.StartedDate <= end);
                    }
                    break;
            }

            ViewBag.Filter = filter;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(bookings.ToList());
        }




        // GET: Booking/Create or Edit
        public async Task<IActionResult> Create(int? id)
        {
            ViewBag.Services = await _context.Services.ToListAsync();

            if (id == null || id == 0)
                return View(new BookingViewModel());

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            var model = new BookingViewModel
            {
                ServiceId = booking.ServiceId,
                StartedDate = booking.StartedDate,
                StartedTime = booking.StartedTime,
                Topic = booking.Topic,
                Notes = booking.Notes
            };

            ViewBag.BookingId = booking.UniqueId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, BookingViewModel model)
        {
            ViewBag.Services = await _context.Services.ToListAsync();
            ViewBag.BookingId = id;

            if (!ModelState.IsValid)
                return View(model);

            if (id == null || id == 0)
            {
                // CREATE
                var booking = new Booking
                {
                    ServiceId = model.ServiceId,
                    StartedDate = model.StartedDate,
                    StartedTime = model.StartedTime,
                    Topic = model.Topic,
                    Notes = model.Notes,
                    EndedDate = null
                };
                _context.Booking.Add(booking);
            }
            else
            {
                // UPDATE
                var booking = await _context.Booking.FindAsync(id);
                if (booking == null) return NotFound();

                booking.ServiceId = model.ServiceId;
                booking.StartedDate = model.StartedDate;
                booking.StartedTime = model.StartedTime;
                booking.Topic = model.Topic;
                booking.Notes = model.Notes;

                _context.Booking.Update(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // DELETE methods (optional if you're keeping Delete)
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Booking
                .Include(b => b.Service)
               
                .FirstOrDefaultAsync(b => b.UniqueId == id);

            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
