using JobApplication.Models;

namespace JobApplication.DTOs.Application
{
    public class PatchApplicationDto: ApplicationBaseDto
    {
        public ApplicationStatus? Status { get; set; }
    }
}

