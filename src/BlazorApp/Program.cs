using BlazorApp.Components;
using Client;
using Radzen;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/blazorapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication
builder.Services.AddAuthenticationCore();
builder.Services.AddAuthorizationCore();

// Add Radzen
builder.Services.AddRadzenComponents();

// Add Client services
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5021";
builder.Services.AddClient(apiBaseUrl);

// Add Application services
builder.Services.AddScoped<BlazorApp.Services.ICurrentUserService, BlazorApp.Services.CurrentUserService>();
builder.Services.AddScoped<BlazorApp.Services.IUserPreferencesService, BlazorApp.Services.UserPreferencesService>();
builder.Services.AddScoped<BlazorApp.Services.IThemeService, BlazorApp.Services.ThemeService>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

// Map health checks
app.MapHealthChecks("/health");

app.Run();
