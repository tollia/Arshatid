using ArshatidPublic.Classes;
using System.Net.Http.Headers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TokenHandler>();
builder.Services.AddSingleton<ITokenHandlerService, IslandIsTokenHandlerService>();
builder.Services.AddScoped<ManualJwtSignInFilter>();
builder.Services.AddTransient<ApiTokenHandler>();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("ArshatidApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ArshatidApi:BaseUrl"]!);
    var token = builder.Configuration["ArshatidApi:Jwt"];
    if (!string.IsNullOrWhiteSpace(token))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
