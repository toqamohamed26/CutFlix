using CUTFLI.ActionFilter;
using CUTFLI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using NLog.Web;
using NToastNotify;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();
builder.Logging.AddNLog();


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Listen on port 5000
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder
            .AllowAnyOrigin() 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("sqlConnection");

builder.Services.AddMvc().AddNToastNotifyToastr(new ToastrOptions()
{
    ProgressBar = true,
    PositionClass = ToastPositions.TopRight,
    PreventDuplicates = true,
    CloseButton = true,
});

builder.Services.AddDbContext<CUTFLIDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AdminFilter>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "CookieCUTFLI";
        option.LoginPath = "/Account/Login";
        option.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
});

// builder.Services.AddHangfire(x => x
// .UseSimpleAssemblyNameTypeSerializer()
// .UseRecommendedSerializerSettings()
// .UseSqlServerStorage(connectionString));

//builder.Services.AddHangfireServer();

//builder.Services.AddTransient<IServiceManagment, ServiceManagment>();


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithRedirects("/CutFli");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();


// app.UseHangfireDashboard("/hangfire",new DashboardOptions()
// {
//     DashboardTitle = "CutFlix - Dashboard",
//     Authorization = new []
//     {
//         new HangfireCustomBasicAuthenticationFilter()
//         {
//             Pass = "Amer111@",
//             User = "cutflix"
//         }
//     }
// });
//app.MapHangfireDashboard();

//RecurringJob.AddOrUpdate<IServiceManagment>(x => x.CheckAppointmentsTime(), "0 * * ? * *");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CutFli}/{action=Index}/{id?}");
app.Run();
