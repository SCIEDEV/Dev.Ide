using System.Net;
using System.Reflection;
using Dev.Ide.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Dev.Ide.Worker;
using static System.Net.WebRequestMethods;

Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Buffer");

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSignalR();
builder.Services.AddHostedService<DbHostedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://oj.tech4good.tech/",
                "https://code.sciedev.com",
                "https://oj-api.tech4good.tech/").AllowAnyHeader().AllowAnyMethod().WithMethods("GET", "POST")
                .AllowCredentials().SetIsOriginAllowed(_ => true);
        });
});

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

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<TerminalHub>("/terminalHub");

app.Run();

