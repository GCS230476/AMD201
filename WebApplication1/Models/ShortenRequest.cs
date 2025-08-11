using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ShortenRequest
    {
        [Required, Url, MaxLength(2048)]
        public string OriginalUrl { get; set; } = string.Empty;

        // Optional: allow custom code (nice & meaningful)
        [RegularExpression("^[a-zA-Z0-9_-]{3,30}$", ErrorMessage = "Custom code must be 3-30 chars: letters, numbers, _ or - only.")]
        public string? CustomCode { get; set; }
    }
}
