using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExchangeWebsite.Controllers;
using ExchangeWebsite.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ExchangeWebsiteContextConnection")
    ?? throw new InvalidOperationException("Connection string 'ExchangeWebsiteContextConnection' not found.");

// Register the application's DbContext
builder.Services.AddDbContext<ExchangeWebsiteContext>(options =>
    options.UseSqlServer(connectionString));

// Register Identity with the custom User class
builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        // You can add more Identity options here if needed
    })
    .AddRoles<IdentityRole>() // <-- This is required!
    .AddEntityFrameworkStores<ExchangeWebsiteContext>();

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHostedService<PostCleanupService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

Directory.CreateDirectory("/var/data/uploads");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider("/var/data/uploads"),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseAuthentication(); // Ensure authentication middleware is added
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// The 'public' modifier is not valid for local functions. To fix the error, remove the 'public' modifier from the SeedRolesAsync method.

using (var scope = app.Services.CreateScope())
{
    await SeedRolesAsync(scope.ServiceProvider);
}

static async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Admin", "Shipper" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
app.Run();

