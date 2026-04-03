using JobApplication.Models;

namespace JobApplication.DTOs.Application
{
    public class PatchDto
    {
        public ApplicationStatus? Status { get; set; }
    }
}
