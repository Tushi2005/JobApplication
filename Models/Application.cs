namespace JobApplication.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public required string CompanyName { get; set; }
        public required string Position { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Sent;
        public DateTime AppliedAt { get; set; }
        public DateTime? InterviewAt { get; set; }
        public string? JobUrl { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
