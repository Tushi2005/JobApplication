using System.ComponentModel.DataAnnotations;

namespace JobApplication.DTOs.Application
{
    public abstract class ApplicationBaseDto
    {
        [Required(ErrorMessage = "A cégnév megadása kötelező!")]
        [MaxLength(30, ErrorMessage = "A cégnév maximum 30 karakter lehet!")]
        public required string CompanyName { get; set; }

        [Required(ErrorMessage = "A pozíció megadása kötelező!")]
        public required string Position { get; set; }
        public DateTime AppliedAt { get; set; }

        public DateTime? InterviewAt { get; set; }

        [Url(ErrorMessage = "Érvénytelen URL formátum!")]
        public string? JobUrl { get; set; }

        public string? Notes { get; set; }
    }
}
