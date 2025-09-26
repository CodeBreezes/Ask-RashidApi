using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using BookingAppAPI.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            var bookingsQuery = _context.Booking
                .Include(b => b.Service)
              
                .AsQueryable();

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (!string.IsNullOrEmpty(filter))
            {
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
            }

            if (serviceId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.ServiceId == serviceId.Value);
            }

            // ✅ ensure proper ordering by date + time
            bookingsQuery = bookingsQuery
                .OrderByDescending(b => b.StartedDate)
                .ThenByDescending(b => b.StartedTime);

            ViewBag.Filter = filter;
            ViewBag.ServiceId = serviceId;
            ViewBag.Services = await _context.Services.ToListAsync();

            return View(await bookingsQuery.ToListAsync());
        }

public async Task<IActionResult> Details(int id)
    {
        // Booking fetch karo
        var booking = await _context.Booking
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.UniqueId == id);

        if (booking == null) return NotFound();

        // User fetch karo separately
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.UniqueId == booking.UserId);

        if (user == null) return NotFound();

        // Map to ViewModel
        var model = new BookingDetailsViewModel
        {
            // Booking info
            UniqueId = booking.UniqueId,
            ServiceName = booking.Service?.Name ?? "",
            StartedDate = booking.StartedDate,
            StartedTime = booking.StartedTime,
            EndedDate = booking.EndedDate,
            Topic = booking.Topic,
            Notes = booking.Notes,

            // User info
            UserUniqueId = user.UniqueId,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            ProfileImageUrl = user.ProfileImageUrl,
            Address = user.Address
        };

        return View(model);
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



