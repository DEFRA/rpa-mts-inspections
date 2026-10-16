using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.IIS;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Helpers;
using RPA.MTSInspections.Migrations;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.SL.Interfaces;
using System.Data.Entity.Migrations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MTSInspectionsContext")
    ?? throw new InvalidOperationException("Connection string 'MTSInspectionsContext' is not configured.");

// IIS in-process hosting authenticates the user itself; Kestrel needs the Negotiate handler.
var hostedInIis = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APP_POOL_ID"));
if (hostedInIis)
{
    builder.Services.AddAuthentication(IISServerDefaults.AuthenticationScheme);
}
else
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IClaimsTransformation, IdtRoleClaimsTransformation>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped(_ => new MTSInspectionsContext(connectionString));
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICriteriaService, CriteriaService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<ISchemeService, SchemeService>();

if (!string.IsNullOrEmpty(builder.Configuration["ApplicationInsights:ConnectionString"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddControllersWithViews();

var app = builder.Build();

UserHelper.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

// Schema changes are normally applied by the deployment pipeline; this switch keeps the previous
// "migrate on start-up" behaviour available for environments that still need it.
if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    new DbMigrator(new Configuration(connectionString)).Update();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-GB")
    .AddSupportedCultures("en-GB")
    .AddSupportedUICultures("en-GB"));

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
