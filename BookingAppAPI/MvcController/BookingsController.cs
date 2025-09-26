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
        {
            return View();
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

          

            // ===============================
            // INDEX: Basic Payments
            // ===============================
          
            // ===============================
            public async Task<IActionResult> Payment(string searchString, string sortOrder, string filter)
            {
            ViewBag.Filter = filter ?? "";
            ViewData["CurrentSort"] = sortOrder;

                var payments = await _context.Payments.ToListAsync();

                var uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"); // GMT+4

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
                        Gender = user?.Gender,
                        ProfileImageUrl = user?.ProfileImageUrl,
                        Address = user?.Address
                    };
                }).AsQueryable();

                // 🔍 Search
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

                // 📅 Filter
                if (!string.IsNullOrEmpty(filter))
                {
                    switch (filter)
                    {
                        case "today":
                            paymentVMs = paymentVMs.Where(p => p.CreatedAt.Date == DateTime.UtcNow.AddHours(4).Date);
                            break;
                        case "thisWeek":
                            var now = DateTime.UtcNow.AddHours(4);
                            int diff = (int)now.DayOfWeek;
                            var weekStart = now.AddDays(-diff);
                            var weekEnd = weekStart.AddDays(7).AddSeconds(-1);
                            paymentVMs = paymentVMs.Where(p => p.CreatedAt >= weekStart && p.CreatedAt <= weekEnd);
                            break;
                        case "thisMonth":
                            var today = DateTime.UtcNow.AddHours(4);
                            paymentVMs = paymentVMs.Where(p => p.CreatedAt.Month == today.Month && p.CreatedAt.Year == today.Year);
                            break;
                        case "thisYear":
                            var current = DateTime.UtcNow.AddHours(4);
                            paymentVMs = paymentVMs.Where(p => p.CreatedAt.Year == current.Year);
                            break;
                    }
                }

                // ↕️ Sort
                paymentVMs = sortOrder switch
                {
                    "date_desc" => paymentVMs.OrderByDescending(p => p.CreatedAt),
                    "Date" => paymentVMs.OrderBy(p => p.CreatedAt),
                    _ => paymentVMs.OrderByDescending(p => p.CreatedAt)
                };

                return View(paymentVMs.ToList());
            }

            // ===============================
            // DETAILS: Full Payment Info
            // ===============================
            public async Task<IActionResult> PaymentDetails(int id)
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
                if (payment == null) return NotFound();

                var service = payment.ServiceId.HasValue ? _context.Services.FirstOrDefault(s => s.UniqueId == payment.ServiceId.Value) : null;
                var user = payment.userId.HasValue ? _context.AppUsers.FirstOrDefault(u => u.UniqueId == payment.userId.Value) : null;

                var uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"); // GMT+4

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
                    UserId = payment.userId,
                    UserUniqueId = user?.UniqueId ?? 0,
                    FullName = user?.FullName ?? "",
                    UserPhoneNumber = user?.PhoneNumber,
                    UserEmail = user?.Email,
                    DateOfBirth = user?.DateOfBirth,
                    Gender = user?.Gender,
                    ProfileImageUrl = user?.ProfileImageUrl,
                    Address = user?.Address
                };

                return View(paymentVM);
            }
        }
    }





