using Microsoft.EntityFrameworkCore;
using Status;
using Status.Data;
using Status.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHostedService<MuebStatusWorker>();
builder.Services.AddDbContext<SchmatrixDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("SchmatrixConnection"))
                    .UseSnakeCaseNamingConvention());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.MapHub<StatusHub>("/hubs/status");

app.Run();
