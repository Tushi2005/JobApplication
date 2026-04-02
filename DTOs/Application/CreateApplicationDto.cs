using System.ComponentModel.DataAnnotations;

namespace JobApplication.DTOs.Application
{
    public class CreateApplicationDto
    {
        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(30, ErrorMessage = "Company name too long")]
        public required string CompanyName { get; set; }

        [Required(ErrorMessage = "Position is required")]
        public required string Position { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? InterviewAt { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        public string? JobUrl { get; set; }
        public string? Notes
        {
            get; set;
        }
    }
}
