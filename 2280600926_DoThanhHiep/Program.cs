using _2280600926_DoThanhHiep.Models;
using _2280600926_DoThanhHiep.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>

options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate(); // Đảm bảo database đã cập nhật

    if (!context.Categories.Any()) // Nếu chưa có danh mục nào
    {
        context.Categories.AddRange(new List<Category>
        {
            new Category { Name = "Điện tử" },
            new Category { Name = "Thời trang" },
            new Category { Name = "Đồ gia dụng" }
        });

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Đã thêm danh mục vào database!");
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
