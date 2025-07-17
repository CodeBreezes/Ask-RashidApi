namespace BookingAppAPI.DB.Models
{
    public class ServiceFullViewModel
    {
        public int? UniqueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int Cost { get; set; }

        public List<SubtopicViewModel> Subtopics { get; set; } = new();
    }

    public class SubtopicViewModel
    {
        public string Title { get; set; } = string.Empty;
        public List<BulletinViewModel> Bulletins { get; set; } = new();
    }

    public class BulletinViewModel
    {
        public string Content { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

}
