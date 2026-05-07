using JobApplication.Models;

namespace JobApplication.DTOs.Application
{
    public class UpdateApplicationDto: ApplicationBaseDto
    {
        public ApplicationStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
