using Erp_Aits.Services;
using Erp_Aits.Infrastructure.ErpNext;
using Erp_Aits.Infrastructure.ErpNext.MasterData;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
System.Net.ServicePointManager.Expect100Continue = false;

// Add services
builder.Services.AddControllersWithViews();

// Database connection
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

builder.Services.Configure<ErpNextOptions>(builder.Configuration.GetSection("ErpNext"));
builder.Services.AddSingleton<IErpNextAuditLogger, FileErpNextAuditLogger>();
builder.Services.AddHttpClient<IErpNextClient, ErpNextClient>();
builder.Services.AddScoped<IErpNextMasterDataService, ErpNextMasterDataService>();
builder.Services.AddHttpClient<Erp_Aits.Services.ErpNextService>();
// ============================================
// REGISTER PAGE ACCESS SERVICE - ADD THIS
// ============================================
builder.Services.AddScoped<IPageAccessService, PageAccessService>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// WhatsApp Service
builder.Services.AddHttpClient<WhatsAppService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

builder.Services.AddScoped<WhatsAppService>();

// Campaign Scheduler
builder.Services.AddScoped<Erp_Aits.Controllers.CampaignsController>();
// Campaign delivery is not needed while testing the local HRMS screens. It can
// otherwise interrupt the debugger when its optional database tables are absent.
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<CampaignSchedulerService>();
}

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
