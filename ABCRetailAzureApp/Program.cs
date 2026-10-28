using ABCRetailAzureApp.Services;
using ABCRetailAzureApp.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddScoped<IAzureTableService<Product>>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var tableName = configuration["AzureStorage:Tables:Products"] ?? "products";
    return new AzureTableService<Product>(configuration, tableName);
});

builder.Services.AddScoped<IAzureTableService<CustomerProfile>>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var tableName = configuration["AzureStorage:Tables:CustomerProfiles"] ?? "customerprofiles";
    return new AzureTableService<CustomerProfile>(configuration, tableName);
});

builder.Services.AddScoped<IAzureTableService<Order>>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var tableName = configuration["AzureStorage:Tables:Orders"] ?? "orders";
    return new AzureTableService<Order>(configuration, tableName);
});


builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IAzureQueueService, AzureQueueService>();
builder.Services.AddScoped<IAzureFileService, AzureFileService>();


builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();