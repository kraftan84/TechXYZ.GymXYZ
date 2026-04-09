using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using TechXyz.GymXyz.Application.Extensions;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;
using TechXyz.GymXyz.Persistence.Data;
using TechXyz.GymXyz.Persistence.Extensions;
using TechXyz.GymXyz.WebApp.Components;
using TechXyz.GymXyz.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();
builder.Services.AddDataGridEntityFrameworkAdapter();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<ICurrentUserOverride, CurrentUserOverride>();
builder.Services.AddScoped<BreadcrumbService>();
builder.Services.AddScoped<IUserFeedbackService, UserFeedbackService>();

builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceLayer(builder.Configuration, builder.Environment);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

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
            DbInitializer.Initialize(serviceProvider, dbContext);
        }
    }

}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
