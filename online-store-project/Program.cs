using online_store_project.Services.BlobStorageServices;
using online_store_project.Services.FileServices;
using online_store_project.Services.QueueServices;
using online_store_project.Services.TableServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ===== SESSION SETUP =====
builder.Services.AddDistributedMemoryCache();          // required
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<TableService>();
builder.Services.AddSingleton<ProductTableService>();
builder.Services.AddSingleton<OrderTableService>();
builder.Services.AddSingleton<ProductFileService>();
builder.Services.AddSingleton<QueueService>();
builder.Services.AddSingleton<IBlobStorageService, ImageStorageService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
