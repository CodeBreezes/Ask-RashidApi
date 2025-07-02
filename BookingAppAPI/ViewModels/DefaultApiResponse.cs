namespace Bpst.API.ViewModels
{
    public class DefaultApiResponse
    {
        public bool IsSuccess { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public List<string>? SuccessMessages { get; set; }
        public List<string>? Details { get; set; }
    }
}
