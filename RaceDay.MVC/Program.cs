using RaceDay.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Session (MVC side – stores user info locally after login) ─────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout         = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly     = true;
    options.Cookie.IsEssential  = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name         = ".RaceDay.MVC.Session";
});

// ── HttpClient → Part 2 API (cookie-based session) ───────────────────────────
// IMPORTANT: UseCookies=true ensures the .RaceDay.Session cookie
// is automatically stored and re-sent on every API call.
// This is how the API knows who is logged in.
var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/";

builder.Services.AddHttpClient<IRaceDayApiService, RaceDayApiService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies              = true,
    CookieContainer         = new System.Net.CookieContainer(),
    AllowAutoRedirect       = false,
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
