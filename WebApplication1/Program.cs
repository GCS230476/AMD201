using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) Read the connection string from appsettings(.Development).json
var conn = builder.Configuration.GetConnectionString("Default");

// 2) Use SQL Server (not SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conn));

builder.Services.AddScoped<UrlService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Optional for dev; if you use migrations you can remove this later
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
