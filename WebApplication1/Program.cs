using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FluentValidation;

using WebApplication1.Data;
using WebApplication1.Services;
using WebApplication1.Validators;

var builder = WebApplication.CreateBuilder(args);

// ------------------ Services ------------------

// DB: SQL Server using "Default" from appsettings.json
var conn = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conn));

// Optional app service (keep if you use it)
builder.Services.AddScoped<UrlService>();

// Caching
builder.Services.AddMemoryCache();

// Rate limiting (example: 5 requests/second for endpoints using this policy)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("shorten-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.QueueLimit = 0;
    });
});

// MVC + FluentValidation
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<ShortenRequestValidator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------ Pipeline ------------------

// Auto-apply EF Core migrations at startup (remove if you prefer manual Update-Database)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve wwwroot (index.html, css, js, etc.)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

// Enable rate limiting middleware
app.UseRateLimiter();

app.UseAuthorization();

// Map controllers (API)
app.MapControllers();

app.Run();