using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using LQV_BlockchainCertificate.Models.Settings;
using LQV_BlockchainCertificate.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 🔥 QUAN TRỌNG: cho phép listen LAN
builder.WebHost.UseUrls("http://0.0.0.0:5187");

// ====================================================================
// ⚙️ 1. SERVICES
// ====================================================================

// ✅ 1.1. DbContext
builder.Services.AddDbContext<LqvDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 1.2. Email
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// ✅ 1.3. Render View & PDF
builder.Services.AddSingleton<IConverter>(
    new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<IViewRenderService, RenderViewService>();

// ✅ 1.4. Ethereum + Hash
builder.Services.AddSingleton<EthereumService>();
builder.Services.AddSingleton<HashHelper>();
builder.Services.AddScoped<ILqvTienDoHocTapService, LqvTienDoHocTapService>();

// ✅ 1.5. HttpClient (CHO GEMINI)
builder.Services.AddHttpClient();

// ====================================================================
// 🤖 GEMINI AI
// ====================================================================

builder.Services.Configure<GeminiSettings>(
    builder.Configuration.GetSection("Gemini"));

builder.Services.AddScoped<GeminiService>();

// ====================================================================
// 🎥 PROCTORING SYSTEM
// ====================================================================

// SignalR
builder.Services.AddSignalR();

// Risk scoring (singleton giữ trạng thái)
builder.Services.AddSingleton<RiskService>();

// Proctor Service
builder.Services.AddScoped<ProctorService>();

// ====================================================================
// 🔐 Authentication
// ====================================================================

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Account/Login";
        options.LogoutPath = "/Auth/Account/Logout";
        options.AccessDeniedPath = "/Auth/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// ✅ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Context
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

// ✅ MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ====================================================================
// 🚀 PIPELINE
// ====================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ❌ TẮT HTTPS nếu test LAN
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ====================================================================
// 🎥 SIGNALR HUB ROUTE
// ====================================================================

app.MapHub<ProctorHub>("/proctorHub");

// ====================================================================
// ROUTES
// ====================================================================

// ✅ Area Route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// ✅ Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();