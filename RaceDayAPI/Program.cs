using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDayAPI.Data;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── CORS – allow the MVC app to call this API ─────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvcPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5001",
                "https://localhost:5001",
                "http://localhost:8080",
                "https://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();   // Required for session cookies
    });
});

// ── Session (server-side auth state) ─────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite    = SameSiteMode.Lax;
    options.Cookie.Name        = ".RaceDay.Session";
    // Allow cross-site in dev (MVC on :5001 calling API on :5000)
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ── MVC / API ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "RaceDay API",
        Version     = "v1",
        Description = "RESTful API for the RaceDay event management system. " +
                      "Supports Organiser and Participant roles via session-based authentication."
    });
});

var app = builder.Build();

// ── Migrate & seed on startup ─────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.Migrate();  ← comment this out for now
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("MvcPolicy");
app.UseSession();
app.MapControllers();

app.Run();

// Required for integration tests
public partial class Program { }
