using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using BookingAppAPI.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Http.HttpResults;

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
        {
            return View();
        }

        // GET: Bookings
        public async Task<IActionResult> Index(int? userId, string? filter, int? serviceId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Base query
            var bookingsQuery = _context.Booking.AsQueryable();

            // ✅ UserId filter lagao
            if (userId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.UserId == userId.Value);
            }

            // ✅ Filter apply karo
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter.ToLower())
                {
                    case "today":
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate == today);
                        break;

                    case "thisweek":
                        var startOfWeek = today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        var endOfWeek = startOfWeek.AddDays(6);
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate >= startOfWeek && b.StartedDate <= endOfWeek);
                        break;

                    case "thismonth":
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate.Month == today.Month && b.StartedDate.Year == today.Year);
                        break;

                    case "thisyear":
                        bookingsQuery = bookingsQuery.Where(b => b.StartedDate.Year == today.Year);
                        break;
                }
            }

            if (serviceId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.ServiceId == serviceId.Value);
            }

            // ✅ Map to ViewModel (LINQ Projection)
            var bookings = await (
                from b in bookingsQuery
                join u in _context.AppUsers on b.UserId equals u.UniqueId
                join s in _context.Services on b.ServiceId equals s.UniqueId
                orderby b.StartedDate descending, b.StartedTime descending
                select new BookingAppAPI.ViewModels.BookingDetailsViewModel
                {
                    // Booking info
                    UniqueId = b.UniqueId,
                    ServiceName = s.Name,
                    StartedDate = b.StartedDate,
                    StartedTime = b.StartedTime,
                    EndedDate = b.EndedDate,
                    Topic = b.Topic,
                    Notes = b.Notes,

                    // User info
                    UserUniqueId = u.UniqueId,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    DateOfBirth = u.DateOfBirth,
                    Gender = u.Gender,
                    ProfileImageUrl = u.ProfileImageUrl,
                    Address = u.Address
                }
            ).ToListAsync();

            // ✅ ViewBags for filters & dropdown
            ViewBag.UserId = userId;
            ViewBag.Filter = filter;
            ViewBag.ServiceId = serviceId;
            ViewBag.Services = await _context.Services.ToListAsync();

            // ✅ IMPORTANT: ab sirf ViewModel bhejna hai
            return View(bookings);
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



        // ===============================
        // INDEX: Basic Payments
        // ===============================

        // ===============================
        public async Task<IActionResult> Payment(string searchString, string filter)
        {
            ViewBag.Filter = filter ?? "";
            ViewData["CurrentFilter"] = searchString ?? "";

            var payments = await _context.Payments.ToListAsync();
            var uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");

            var paymentVMs = payments.Select(p =>
            {
                var service = p.ServiceId.HasValue ? _context.Services.FirstOrDefault(s => s.UniqueId == p.ServiceId.Value) : null;
                var user = p.userId.HasValue ? _context.AppUsers.FirstOrDefault(u => u.UniqueId == p.userId.Value) : null;

                return new PaymentViewModel
                {
                    Id = p.Id,
                    CustomerName = p.CustomerName,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Amount = p.Amount,
                    Currency = "AED",
                    Description = p.Description,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(p.CreatedAt, uaeTimeZone),
                    BookingId = p.BookingId,
                    ServiceId = p.ServiceId,
                    ServiceName = service?.Name ?? "Removed",
                    UserId = p.userId,
                    UserUniqueId = user?.UniqueId ?? 0,
                    FullName = user?.FullName ?? "",
                    UserPhoneNumber = user?.PhoneNumber,
                    UserEmail = user?.Email,
                    DateOfBirth = user?.DateOfBirth,
                    Gender = user.Gender,
                    ProfileImageUrl = user?.ProfileImageUrl,
                    Address = user?.Address,
                    AppointmentDate = p.CreatedAt,
                    Title = service?.Name ?? "Removed",
                    AppointmentTime = p.CreatedAt.ToString("hh:mm tt")


                };
            }).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                paymentVMs = paymentVMs.Where(p =>
                    p.CustomerName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (p.Email != null && p.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchString)) ||
                    (p.BookingId != null && p.BookingId.Contains(searchString)) ||
                    p.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                );
            }

            // Filter
            if (!string.IsNullOrEmpty(filter))
            {
                var now = DateTime.UtcNow.AddHours(4);
                switch (filter)
                {
                    case "today":
                        paymentVMs = paymentVMs.Where(p => p.CreatedAt.Date == now.Date);
                        break;
                    case "thisWeek":
                        int diff = (int)now.DayOfWeek;
                        var weekStart = now.AddDays(-diff);
                        var weekEnd = weekStart.AddDays(7).AddSeconds(-1);
                        paymentVMs = paymentVMs.Where(p => p.CreatedAt >= weekStart && p.CreatedAt <= weekEnd);
                        break;
                    case "thisMonth":
                        paymentVMs = paymentVMs.Where(p => p.CreatedAt.Month == now.Month && p.CreatedAt.Year == now.Year);
                        break;
                    case "thisYear":
                        paymentVMs = paymentVMs.Where(p => p.CreatedAt.Year == now.Year);
                        break;
                }
            }

            // ✅ Alphabetical sort
            paymentVMs = paymentVMs.OrderBy(p => p.CustomerName);

            return View(paymentVMs.ToList());
        }


        // ===============================
        // DETAILS: Full Payment Info
        // ===============================
        public async Task<IActionResult> PaymentDetail(int id)
        {
            // Fetch payment
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return NotFound();

            // Fetch related service
            var service = payment.ServiceId.HasValue
                ? await _context.Services.FirstOrDefaultAsync(s => s.UniqueId == payment.ServiceId.Value)
                : null;

            // Fetch related user
            var user = payment.userId.HasValue
                ? await _context.AppUsers.FirstOrDefaultAsync(u => u.UniqueId == payment.userId.Value)
                : null;

            // Convert CreatedAt to UAE time
            var uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");

            // Map to ViewModel
            var paymentVM = new PaymentViewModel
            {
                Id = payment.Id,
                CustomerName = payment.CustomerName,
                Email = payment.Email,
                PhoneNumber = payment.PhoneNumber,
                Amount = payment.Amount,
                Currency = "AED",
                Description = payment.Description,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(payment.CreatedAt, uaeTimeZone),
                BookingId = payment.BookingId,
                ServiceId = payment.ServiceId,
                ServiceName = service?.Name ?? "Removed",

                // If you have Appointment info stored separately, map it here
                AppointmentDate = payment.CreatedAt,

                AppointmentTime = payment.CreatedAt.ToString("hh:mm tt"),

                // User info
                UserId = payment.userId,
                UserUniqueId = user?.UniqueId ?? 0,
                FullName = user?.FullName ?? "",
                UserPhoneNumber = user?.PhoneNumber,
                UserEmail = payment?.Email,
                DateOfBirth = user?.DateOfBirth,
                Gender = user.Gender,
                ProfileImageUrl = user?.ProfileImageUrl,
                Address = user.Address
            };

            return View(paymentVM);
        }

    }

}





