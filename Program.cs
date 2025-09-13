using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;
using WebTinTuc.Services;

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

// Add EmailService
builder.Services.AddScoped<EmailService>();

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

// Route cụ thể cho các action của BaiViet
app.MapControllerRoute(
    name: "baiviet-manage",
    pattern: "BaiViet/Manage",
    defaults: new { controller = "BaiViet", action = "Manage" });

app.MapControllerRoute(
    name: "baiviet-create",
    pattern: "BaiViet/Create",
    defaults: new { controller = "BaiViet", action = "Create" });

app.MapControllerRoute(
    name: "baiviet-edit",
    pattern: "BaiViet/Edit/{id}",
    defaults: new { controller = "BaiViet", action = "Edit" });

app.MapControllerRoute(
    name: "baiviet-upload",
    pattern: "BaiViet/UploadImage",
    defaults: new { controller = "BaiViet", action = "UploadImage" });

app.MapControllerRoute(
    name: "baiviet-addcomment",
    pattern: "BaiViet/AddComment",
    defaults: new { controller = "BaiViet", action = "AddComment" });

app.MapControllerRoute(
    name: "baiviet-togglecomment",
    pattern: "BaiViet/ToggleCommentStatus",
    defaults: new { controller = "BaiViet", action = "ToggleCommentStatus" });

app.MapControllerRoute(
    name: "baiviet-changestatus",
    pattern: "BaiViet/ChangeStatus",
    defaults: new { controller = "BaiViet", action = "ChangeStatus" });

app.MapControllerRoute(
    name: "baiviet-togglehot",
    pattern: "BaiViet/ToggleHot",
    defaults: new { controller = "BaiViet", action = "ToggleHot" });

// Route cho EmailConfirmation
app.MapControllerRoute(
    name: "email-confirmation",
    pattern: "Account/EmailConfirmation",
    defaults: new { controller = "Account", action = "EmailConfirmation" });

// Route cho slug bài viết
app.MapControllerRoute(
    name: "baiviet",
    pattern: "BaiViet/{slug}",
    defaults: new { controller = "BaiViet", action = "Details" });

// Route cụ thể cho các action của ChuDe
app.MapControllerRoute(
    name: "chude",
    pattern: "ChuDe/{slug?}",
    defaults: new { controller = "ChuDe", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
