using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WebTinTucContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("WebTinTucDB"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "baiviet",
    pattern: "BaiViet/{slug?}",
    defaults: new { controller = "BaiViet", action = "Details" });

app.MapControllerRoute(
    name: "chude",
    pattern: "ChuDe/{slug?}",
    defaults: new { controller = "ChuDe", action = "Index" });
    

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
