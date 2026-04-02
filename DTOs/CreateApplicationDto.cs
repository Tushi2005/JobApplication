namespace JobApplication.DTOs
{
    public class CreateApplicationDto
    {
        public required string CompanyName { get; set; }
        public required string Position { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? InterviewAt { get; set; }
        public string? JobUrl { get; set; }
        public string? Notes
        {
            get; set;
        }
    }
}
