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
using TechXyz.GymXyz.WebApp.Middleware;
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
builder.Services.Configure<ExternalApiOptions>(
    builder.Configuration.GetSection(ExternalApiOptions.SectionName));

var externalApiOptions = builder.Configuration.GetSection(ExternalApiOptions.SectionName).Get<ExternalApiOptions>()
                         ?? new ExternalApiOptions();

builder.Services.AddHttpClient(SchoolCalendarService.HttpClientName, client =>
{
    // The service cancels itself at the configured budget; this is the backstop
    // behind it, read from the same setting so raising one cannot leave the
    // other cutting the call short.
    client.Timeout = TimeSpan.FromSeconds(externalApiOptions.TimeoutSeconds + 1);
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

// Singleton: the window it holds has to outlive the request that opened it.
builder.Services.AddSingleton<PublicFormThrottle>();

// The deletion promised in the space request's own consent text.
builder.Services.AddHostedService<SpaceRequestPurgeService>();

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

        // The rule the reset screen states out loud: "12 caractères minimum, avec
        // majuscules, minuscules et un chiffre." The hand-off's strength gauge
        // counted from 8 while its own note promised 12 — 12 won, and it is set
        // here so the server enforces exactly what the screen promises. Anything
        // else is a screen lying about the rule.
        options.Password.RequiredLength = 12;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        // Five tries, then a quarter of an hour. Enabled by default so it covers
        // accounts created before this lot: PasswordSignInAsync already asked for
        // lockout on failure, but with nothing configured it never locked.
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
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

// "Le lien est valable 30 minutes" — said on the screen that sends it and in the
// message itself, so the token has to actually expire then. Identity's default
// is a full day.
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromMinutes(30));

// "Les autres appareils ont été déconnectés" is what the confirmation screen
// says, so it has to be true within the minute rather than within Identity's
// default half hour. Resetting a password rolls the security stamp; this is how
// often a cookie is made to notice.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(1));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(GymPolicies.PlatformAdmin, policy => policy.RequireRole(GymRoles.PlatformAdmin));

    // A platform admin used to satisfy this one as well, so that a visit inside
    // a customer could act as its manager. The visit no longer exists, and the
    // exception outlived its reason: an admin who types /reglages is now refused
    // like anybody else who does not run that gym. The console is the whole of
    // what they may open.
    options.AddPolicy(GymPolicies.GymManager, policy => policy.RequireRole(GymRoles.GymManager));
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

// After authentication, so the tenant claim exists; before anything renders, so
// no component has to query for the brand. See TenantResolutionMiddleware.
app.UseTenantResolution();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapAccountEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
