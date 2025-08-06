using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;

namespace BookingAppApi.Controllers
{
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Create()
        { return View();
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string filter, int? serviceId)
        {
            var bookingsQuery = _context.Booking.Include(b => b.Service).AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                switch (filter)
                {
                    case "today":
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate == today);
                        break;

                    case "thisWeek":
                        var startOfWeek = today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        var endOfWeek = startOfWeek.AddDays(6);
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate >= startOfWeek && b.StartedDate <= endOfWeek);
                        break;

                    case "thisMonth":
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate.Month == today.Month && b.StartedDate.Year == today.Year);
                        break;

                    case "thisYear":
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate.Year == today.Year);
                        break;
                }

                ViewBag.Filter = filter;
            }


            if (serviceId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.ServiceId == serviceId.Value);
            }

            ViewBag.Filter = filter;
            ViewBag.ServiceId = serviceId;
        
            ViewBag.Services = await _context.Services.ToListAsync();


            return View(await bookingsQuery.ToListAsync());
        }


        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Booking.FirstOrDefaultAsync(b => b.UniqueId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



            public async Task<IActionResult> Payment(string searchString, string sortOrder, DateTime? startDate, DateTime? endDate)
            {
                ViewData["CurrentFilter"] = searchString;
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

                var payments = _context.Payments.AsQueryable();

                // 🔍 Search logic
                if (!string.IsNullOrEmpty(searchString))
                {
                    payments = payments.Where(p =>
                        p.CustomerName.Contains(searchString) ||
                        p.Email.Contains(searchString) ||
                        p.PhoneNumber.Contains(searchString) ||
                        p.BookingId.Contains(searchString));
                }

                // 📅 Filter by date
                if (startDate.HasValue)
                {
                    payments = payments.Where(p => p.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    payments = payments.Where(p => p.CreatedAt <= endDate.Value);
                }

                // ↕️ Sorting
                switch (sortOrder)
                {
                    case "name_desc":
                        payments = payments.OrderByDescending(p => p.CustomerName);
                        break;
                    case "Date":
                        payments = payments.OrderBy(p => p.CreatedAt);
                        break;
                    case "date_desc":
                        payments = payments.OrderByDescending(p => p.CreatedAt);
                        break;
                    default:
                        payments = payments.OrderBy(p => p.CustomerName);
                        break;
                }

                var result = await payments.ToListAsync();
                return View(result);
            }
        }
    }



