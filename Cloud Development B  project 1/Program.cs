using Cloud_Development_B__project_1.Functions;
using Cloud_Development_B__project_1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddScoped(
    typeof(ITableStorageService<>),
    typeof(TableStorageService<>));

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IQueueStorageService, QueueStorageService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<CustomerTableFunction>();
builder.Services.AddScoped<ProductImageFunction>();
builder.Services.AddScoped<OrderQueueFunction>();
builder.Services.AddScoped<FileShareFunction>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
