using marketimnet.Data;
using marketimnet.Data.Abstract;
using marketimnet.Data.Concrete;
using marketimnet.Service.Abstract;
using marketimnet.Service.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore.Proxies;
using Microsoft.AspNetCore.StaticFiles;
using marketimnet.wepUI.Hubs;
using marketimnet.wepUI.Areas.Admin.Filters;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use a different port
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5280);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Antiforgery Token Configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add Memory Cache with size limits
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024 * 1024 * 100; // 100 MB
    options.CompactionPercentage = 0.2; // 20% of entries will be removed when size limit is reached
});

// Add Response Compression with Brotli
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "image/svg+xml", "application/json", "text/css", "application/javascript" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Add SignalR
builder.Services.AddSignalR();

// Add Response Caching with optimized settings
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 32 * 1024 * 1024; // 32MB
    options.UseCaseSensitivePaths = true;
});

// Add Output Cache with optimized policies
builder.Services.AddOutputCache(options =>
{
    // Default policy for 10 minutes
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("page", "category", "brand", "search")
        .Tag("default"));

    // Policy for static pages - 1 hour
    options.AddPolicy("Static", builder => builder
        .Expire(TimeSpan.FromHours(1))
        .SetVaryByQuery("none")
        .Tag("static"));

    // Policy for product listings - 5 minutes
    options.AddPolicy("Products", builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .SetVaryByQuery("page", "category", "brand", "search")
        .Tag("products"));
});

try
{
    // Database context içinde
    // Database context
    builder.Services.AddDbContext<DatabaseContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });

    // Statik dosya yapılandırması
    builder.Services.Configure<StaticFileOptions>(options =>
    {
        options.OnPrepareResponse = ctx =>
        {
            const int durationInSeconds = 60 * 60 * 24 * 30; // 30 days
            ctx.Context.Response.Headers["Cache-Control"] = 
                $"public,max-age={durationInSeconds}";
        };
    });

    // Services
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Repository registrations
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    // Add AdminAuthFilter
    builder.Services.AddScoped<AdminAuthFilter>();

    // Add session services with extended timeout
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

    // Add authentication with extended timeout
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Admin/Admin/Index";
            options.LogoutPath = "/Admin/Admin/Logout";
            options.AccessDeniedPath = "/Admin/Admin/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(3);
        });
}
catch (Exception ex)
{
    Console.WriteLine($"Error during service configuration: {ex.Message}");
    throw;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable Response Compression
app.UseResponseCompression();

app.UseHttpsRedirection();

// Önceki StaticFileOptions yapılandırmasını kaldırın
// builder.Services.Configure<StaticFileOptions> ve builder.Services.AddStaticFiles kısımlarını silin

// Sadece bu kısmı ekleyin
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 30; // 30 days
        ctx.Context.Response.Headers["Cache-Control"] = 
            $"public,max-age={durationInSeconds}";
    }
});

app.UseRouting();

// Enable Response Caching
app.UseResponseCaching();

app.UseOutputCache();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Admin route'u ekle
app.MapControllerRoute(
    name: "admin",
    pattern: "admin",
    defaults: new { area = "Admin", controller = "Admin", action = "Index" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
