using WebApplication1.Services;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using Microsoft.AspNetCore.RateLimiting;   // <- add
using System.Threading.RateLimiting;       // <- add
using FluentValidation;                    // <- add
using WebApplication1.Validators;          // <- namespace for your validator

var builder = WebApplication.CreateBuilder(args);

// SQL Server connection (you already set this)
var conn = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<WebApplication1.Data.AppDbContext>(options =>
    options.UseSqlServer(conn));


builder.Services.AddScoped<UrlService>(); // if you still have it (ok to keep/not use)

// 🔹 Add memory cache
builder.Services.AddMemoryCache();

// 🔹 Add rate limiting (5 requests/sec for the shorten endpoint)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("shorten-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.QueueLimit = 0;
    });
});

builder.Services.AddControllers();

// 🔹 Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<ShortenRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Program.cs (after app = builder.Build())
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // <-- applies migrations automatically on startup
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

// 🔹 Enable rate limiter middleware
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
