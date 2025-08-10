using System;
using System.ComponentModel.DataAnnotations;


namespace WebApplication1.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        [Required]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        public string ShortCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
