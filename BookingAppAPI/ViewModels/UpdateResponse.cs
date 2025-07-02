namespace Bpst.API.ViewModels
{
    public class UpdateResponse
    {
        public bool IsUpdated { get; set; }
        public string? UniqueId { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public List<string>? SuccessMessages { get; set; }
        public List<string>? Details { get; set; }
    }
}
