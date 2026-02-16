using InternExcelTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Add Services
// --------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logger Service
builder.Services.AddScoped<
    InternExcelTracker.Api.Services.ILoggerService,
    InternExcelTracker.Api.Services.FileLoggerService>();

// --------------------
// Database Configuration
// --------------------

// Render provides DATABASE_URL
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --------------------
// CORS Configuration (Vercel Frontend)
// --------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy
                .WithOrigins("https://intern-excel-tracker.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// --------------------
// Middleware Pipeline
// --------------------

// Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Order matters
app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

// --------------------
// Render Port Binding
// --------------------

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
