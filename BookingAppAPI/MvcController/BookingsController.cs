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
using BookingAppAPI.DB.Models;

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


        public IActionResult Payment(string? searchString, string? filter)
        {
            var paymentsQuery = from p in _context.Payments
                                join u in _context.AppUsers on p.userId equals u.UniqueId into userGroup
                                from u in userGroup.DefaultIfEmpty()
                                join s in _context.Services on p.ServiceId equals s.UniqueId into serviceGroup
                                from s in serviceGroup.DefaultIfEmpty()
                                select new PaymentViewModel
                                {
                                    Id = p.Id,
                                    StripePaymentIntentId = string.IsNullOrWhiteSpace(p.StripePaymentIntentId) ? "No Stripe ID" : p.StripePaymentIntentId,
                                    CustomerName = u != null && !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : (!string.IsNullOrWhiteSpace(p.CustomerName) ? p.CustomerName : "No Name"),
                                    FullName = u != null && !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : (!string.IsNullOrWhiteSpace(p.CustomerName) ? p.CustomerName : "No Name"),
                                    UserId = p.userId,
                                    UserUniqueId = u.UniqueId ,
                                    Email = string.IsNullOrWhiteSpace(u.Email) ? "No Record" : u.Email,
                                    UserPhoneNumber = string.IsNullOrWhiteSpace(u.PhoneNumber) ? "No Phone" : u.PhoneNumber,
                                    ServiceId = p.ServiceId,
                                    ServiceName = s != null && !string.IsNullOrWhiteSpace(s.Name) ? s.Name : "No Service",
                                    AppointmentDate = p.CreatedAt,
                                    AppointmentTime = p.CreatedAt.ToString("hh:mm tt"),
                                    Amount = p.Amount,
                                    Currency = string.IsNullOrWhiteSpace(p.Currency) ? "AED" : p.Currency,
                                    Description = string.IsNullOrWhiteSpace(p.Description) ? "No Description" : p.Description,
                                    CreatedAt = p.CreatedAt,
                                    BookingId = string.IsNullOrWhiteSpace(p.BookingId) ? "No Booking" : p.BookingId
                                };

            // Optional search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                paymentsQuery = paymentsQuery.Where(p =>
                    (p.FullName != null && p.FullName.Contains(searchString)) ||
                    (p.ServiceName != null && p.ServiceName.Contains(searchString)) ||
                    (p.BookingId != null && p.BookingId.Contains(searchString))
                );
            }

            // Optional date filter
            if (!string.IsNullOrEmpty(filter))
            {
                var now = DateTime.UtcNow;
                paymentsQuery = filter switch
                {
                    "today" => paymentsQuery.Where(p => p.CreatedAt.Date == now.Date),
                    "thisWeek" => paymentsQuery.Where(p => (now - p.CreatedAt).TotalDays <= 7),
                    "thisMonth" => paymentsQuery.Where(p => p.CreatedAt.Month == now.Month && p.CreatedAt.Year == now.Year),
                    "thisYear" => paymentsQuery.Where(p => p.CreatedAt.Year == now.Year),
                    _ => paymentsQuery
                };
            }

            var payments = paymentsQuery.ToList();
            return View(payments);
        }



        // ===============================
        // DETAILS: Full Payment Info
        // ===============================
      
            // Map to ViewModel safely
         public async Task<IActionResult> PaymentDetail(int id)
        {
            // Fetch payment
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return NotFound("No payment record found.");

            // Fetch related service
            var service = payment.ServiceId.HasValue
                ? await _context.Services.FirstOrDefaultAsync(s => s.UniqueId == payment.ServiceId.Value)
                : null;

            // Fetch related user
            var user = payment.userId.HasValue
                ? await _context.AppUsers.FirstOrDefaultAsync(u => u.UniqueId == payment.userId.Value)
                : null;

            var uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");

            // Map to ViewModel safely
            var paymentVM = new PaymentViewModel
            {
                Id = payment.Id,
                CustomerName = string.IsNullOrWhiteSpace(user?.FullName) ? (string.IsNullOrWhiteSpace(payment.CustomerName) ? "No Name" : payment.CustomerName) : user.FullName,
                Email = string.IsNullOrWhiteSpace(payment.Email) ? "No Email" : payment.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(user?.PhoneNumber) ? "No Phone" : user.PhoneNumber,
                Amount = payment.Amount,
                Currency = string.IsNullOrWhiteSpace(payment.Currency) ? "AED" : payment.Currency,
                Description = string.IsNullOrWhiteSpace(payment.Description) ? "No Description" : payment.Description,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(payment.CreatedAt, uaeTimeZone),
                BookingId = string.IsNullOrWhiteSpace(payment.BookingId) ? "No Booking" : payment.BookingId,
                ServiceId = payment.ServiceId,
                ServiceName = string.IsNullOrWhiteSpace(service?.Name) ? "No Service" : service.Name,
                AppointmentDate = payment.CreatedAt,
                AppointmentTime = payment.CreatedAt.ToString("hh:mm tt"),
                UserId = payment.userId,
                UserUniqueId = user?.UniqueId ?? 0,
                FullName = string.IsNullOrWhiteSpace(user?.FullName) ? "No Name" : user.FullName,
                UserPhoneNumber = string.IsNullOrWhiteSpace(user?.PhoneNumber) ? "No Phone" : user.PhoneNumber,
                UserEmail = string.IsNullOrWhiteSpace(user?.Email) ? "No Email" : user.Email,
                DateOfBirth = user?.DateOfBirth,
                Gender = string.IsNullOrWhiteSpace(user?.Gender) ? "No Gender" : user.Gender,
                ProfileImageUrl = string.IsNullOrWhiteSpace(user?.ProfileImageUrl) ? "/images/default-profile.png" : user.ProfileImageUrl,
                Address = string.IsNullOrWhiteSpace(user?.Address) ? "No Address" : user.Address
            };

            return View(paymentVM);
        }


    }

}





