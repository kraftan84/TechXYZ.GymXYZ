using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using TechXyz.GymXyz.Application.Extensions;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;
using TechXyz.GymXyz.Persistence.Data;
using TechXyz.GymXyz.Persistence.Extensions;
using TechXyz.GymXyz.Persistence.Identity;
using TechXyz.GymXyz.WebApp.Components;
using TechXyz.GymXyz.WebApp.Services;

// The product ships in French only: dates, numbers and the Fluent controls all
// read the same culture, whatever locale the host happens to run under.
CultureInfo.DefaultThreadCurrentCulture = GxFormats.Culture;
CultureInfo.DefaultThreadCurrentUICulture = GxFormats.Culture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();
builder.Services.AddMemoryCache();

// The school calendar is the one thing this application fetches from outside.
// Named client so its own timeout and headers stay off everybody else's.
builder.Services.AddHttpClient(SchoolCalendarService.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("GymXYZ/1.0");
});
builder.Services.AddScoped<ISchoolCalendarService, SchoolCalendarService>();

// Outgoing mail. Which implementation is registered depends on whether a key is
// configured: without one nothing leaves, so a development machine pointed at a
// copy of production cannot e-mail real members.
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddHttpClient(BrevoEmailSender.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

var hasEmailProvider = !string.IsNullOrWhiteSpace(builder.Configuration[$"{EmailOptions.SectionName}:ApiKey"]);

if (hasEmailProvider)
{
    builder.Services.AddScoped<IEmailSender, BrevoEmailSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();
}
builder.Services.AddDataGridEntityFrameworkAdapter();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<ICurrentUserService>(provider => provider.GetRequiredService<CurrentUserService>());
builder.Services.AddSingleton<ICurrentUserOverride, CurrentUserOverride>();
builder.Services.AddScoped<BreadcrumbService>();
builder.Services.AddScoped<IUserFeedbackService, UserFeedbackService>();

// Multi-tenant: one holder per scope, filled by TenantBoundary from the signed-in
// user's claims (host prefix before authentication).
var tenantOptions = builder.Configuration.GetSection(TenantOptions.SectionName).Get<TenantOptions>()
                    ?? new TenantOptions();
builder.Services.AddSingleton(tenantOptions);
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<ResponsiveModeService>();
builder.Services.AddScoped<MobileHeaderService>();
builder.Services.AddScoped<NavBadgeService>();

builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceLayer(builder.Configuration, builder.Environment);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity: local accounts, cookie authentication, no public registration.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<GymDbContext>()
    .AddSignInManager()
    .AddClaimsPrincipalFactory<GymUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/connexion";
    options.LogoutPath = "/account/deconnexion";
    options.AccessDeniedPath = "/account/acces-refuse";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(GymPolicies.PlatformAdmin, policy => policy.RequireRole(GymRoles.PlatformAdmin));
    options.AddPolicy(GymPolicies.GymManager, policy => policy.RequireRole(GymRoles.GymManager, GymRoles.PlatformAdmin));
});

var app = builder.Build();

// Said out loud at startup: "why did no member get the cancellation e-mail" is
// otherwise answered by reading the configuration of a running server.
app.Logger.LogInformation(
    hasEmailProvider
        ? "Outgoing e-mail: Brevo, sending from {From}."
        : "Outgoing e-mail: no provider configured — messages are written to the log and not sent.",
    builder.Configuration[$"{EmailOptions.SectionName}:FromAddress"]);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<GymDbContext>();
    var resetDatabaseOnStartup = app.Configuration.GetValue<bool>("ResetDatabaseOnStartup");

    if (app.Environment.IsDevelopment())
    {
        // Reset the dev database only when explicitly enabled.
        if (resetDatabaseOnStartup)
        {
            await dbContext.Database.EnsureDeletedAsync();
        }

        await dbContext.Database.EnsureCreatedAsync();

        if (resetDatabaseOnStartup)
        {
            await DbInitializer.InitializeAsync(serviceProvider, dbContext);
        }
    }

}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapAccountEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
