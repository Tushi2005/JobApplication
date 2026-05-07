using JobApplication.Models;

namespace JobApplication.DTOs.Application
{
    public class ApplicationResponseDto: ApplicationBaseDto
    {
        public int Id { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
