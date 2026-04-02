using JobApplication.Models;

namespace JobApplication.DTOs.Application
{
    public class UpdateApplicationDto
    {
        public required string CompanyName { get; set; }
        public required string Position { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? InterviewAt { get; set; }
        public string? JobUrl { get; set; }
        public string? Notes { get; set; }
    }
}
