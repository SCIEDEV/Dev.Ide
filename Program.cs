using System.Net;
using System.Reflection;
using Dev.Ide.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Dev.Ide.Worker;

Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Buffer");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSignalR();
builder.Services.AddHostedService<DbHostedService>();

if (builder.Environment.IsDevelopment())
{

}
else
{
    builder.WebHost.UseKestrel(options =>
    {
        options.Listen(IPAddress.Any, 10234);
    });
}

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<TerminalHub>("/terminalHub");

app.Run();

