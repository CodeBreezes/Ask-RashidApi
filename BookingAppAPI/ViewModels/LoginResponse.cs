using BookingAppAPI.DB.Models.User;

namespace Bpst.API.ViewModels
{
    public class LoginResponse
    {
        public List<Roles>? userRoles; 
 
        public bool IsLoginSuccess { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string>? ErrorMessages { get; set; }
        public string FName { get; set; } = string.Empty;
        public string LName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string UserGroups { get; set; } = string.Empty;
        public string DivisionName { get; set; } = string.Empty;
        public string DivisionDescription { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentDescription { get; set; } = string.Empty;
        public Guid DivisionId { get; set; }
        public Guid DepartmentId { get; set; }
    }
}
