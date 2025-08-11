using System;                       // needed for Uri
using FluentValidation;
using WebApplication1.Models;

namespace WebApplication1.Validators
{
    public class ShortenRequestValidator : AbstractValidator<ShortenRequest>
    {
        public ShortenRequestValidator()
        {
            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("URL is required.")
                .Must(u => Uri.IsWellFormedUriString(u, UriKind.Absolute))
                    .WithMessage("Invalid URL.")
                .Must(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                           u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("URL must start with http:// or https://");

            RuleFor(x => x.CustomCode)
                .Matches("^[a-zA-Z0-9_-]{3,30}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CustomCode))
                .WithMessage("Custom code must be 3–30 chars (letters, numbers, _ or -).");
        }
    }
}
