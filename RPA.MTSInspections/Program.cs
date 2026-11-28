using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Services;

var builder = WebApplication.CreateBuilder(args);

// Schema utilities for the DBA or a pipeline run instead of the web host and need only the connection string.
if (MigrationCommands.IsCommand(args))
{
    return MigrationCommands.Run(args, builder.Configuration.GetConnectionString("MTSInspectionsContext"), Console.Out);
}

// Optional mounted secrets (for example from ACA) override matching environment values.
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);
builder.Services.AddMtsApplication(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseForwardedHeaders();

// Schema changes belong to the DBA / release process. The switch survives for local development only.
if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("ApplyMigrationsOnStartup is only supported in Development; the schema is released separately.");
    }

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<MTSInspectionsContext>().Database.Migrate();
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

// Platform probes use HTTP even when the ingress terminates TLS. Every other route keeps HTTPS redirection.
app.UseWhen(context => context.Request.Path != "/health/live" && context.Request.Path != "/health/ready",
    application => application.UseHttpsRedirection());

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-GB")
    .AddSupportedCultures("en-GB")
    .AddSupportedUICultures("en-GB"));

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Liveness never touches a dependency, so a database outage cannot cause a restart loop.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") })
    .AllowAnonymous();

app.MapRazorPages();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller}/{action}/{id?}");

app.Run();

return 0;

public partial class Program { }
