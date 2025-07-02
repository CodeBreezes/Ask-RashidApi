namespace Bpst.API.ViewModels
{
    public class UserRegistrationResponse
    {
        public bool IsCreated { get; set; }
        public int UniqueId { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public List<string>? SuccessMessages { get; set; }
        public List<string>? Details { get; set; }
    }
}
