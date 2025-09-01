using Arshatid.Authorization;
using Arshatid.Databases;
using Arshatid.Helpers;
using Arshatid.Services;
using GraphAuthentication.Authorization.AccessKey;
using GraphAuthentication.Authorization.Entra;
using GraphAuthentication.Authorization.Graph;
using GraphAuthentication.Authorization.IslandIs;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.Globalization;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Configuration.AddJsonFile("appsettings.Passwords.json", true, true);

CultureInfo cultureInfo = new CultureInfo("is-IS");
cultureInfo.DateTimeFormat.ShortTimePattern = "HH:mm";
cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";
cultureInfo.DateTimeFormat.FullDateTimePattern = "dddd, d. MMMM yyyy HH:mm:ss";

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    RequestCulture defaultCulture = new RequestCulture(cultureInfo);
    IList<CultureInfo> supportedCultures = new List<CultureInfo> { cultureInfo };

    options.DefaultRequestCulture = defaultCulture;
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Force is-IS (ignore Accept-Language, cookies, query, etc.)
    options.RequestCultureProviders.Clear();
});


builder.Services.AddDbContext<GeneralDbContext>();
builder.Services.AddDbContext<ArshatidDbContext>(options =>
{
    options.EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(); // TEMP while diagnosing
        // You can also mirror to Output without a logger factory:
        options.LogTo(message => Debug.WriteLine(message),
            new[]
            {
                RelationalEventId.CommandExecuting,  // SQL + parameters
                RelationalEventId.CommandError,
                RelationalEventId.CommandExecuted
            },
            LogLevel.Information);
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CurrentEventService>();
builder.Services.AddScoped<ClaimsHelper>();
builder.Services.AddScoped<InviteeService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Arshatid API", Version = "v1" });
    options.DocInclusionPredicate((_, api) =>
        api.RelativePath != null &&
        api.RelativePath.StartsWith("islandapi", StringComparison.OrdinalIgnoreCase));
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.Html5DateRenderingMode = Html5DateRenderingMode.Rfc3339;
    });

builder.Services.AddGraphGroupAuthentication(builder.Configuration)
    .AddIslandJwtAuthentication(builder.Configuration)
    .AddAccessKeyJwtAuthentication(builder.Configuration)
    .AddEntraJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IslandIs", policy =>
    {
        policy.AuthenticationSchemes.Add("IslandIs"); // This policy only applies to the "IslandIs" scheme.
        policy.RequireAuthenticatedUser();
    });
    options.AddPolicy("AccessKey", policy =>
    {
        policy.AuthenticationSchemes.Add("AccessKey"); // This policy only applies to the "IslandIs" scheme.
        policy.RequireAuthenticatedUser();
    });
    options.AddPolicy("DevelopmentUser", policy =>
    {
        policy.Requirements.Add(new DevelopmentUserRequirement());
    });
});


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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arshatid API V1");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
