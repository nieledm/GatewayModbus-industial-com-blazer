using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using DL6000WebConfig.Data;
using DL6000WebConfig.Services;
using Microsoft.AspNetCore.Components.Authorization;


var builder = WebApplication.CreateBuilder(args);

// Registra ConfigService primeiro (sem ModbusVariableService)
builder.Services.AddSingleton<ConfigService>(sp =>
{
    var path = Path.Combine(AppContext.BaseDirectory, "DL6000_TO_MODBUS_SLAVE.exe.config");
    return new ConfigService(path);
});

// Registra ModbusVariableService e injeta ConfigService nele
builder.Services.AddSingleton<ModbusVariableService>(sp =>
{
    var configService = sp.GetRequiredService<ConfigService>();
    var modbusService = new ModbusVariableService(Path.Combine(AppContext.BaseDirectory, "variables.json"), configService);
    configService.SetVariableService(modbusService);
    return modbusService;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient("api", client =>
{
    // client.BaseAddress = new Uri("http://localhost:7297");
    client.BaseAddress = new Uri("http://localhost:5055");
    
});
builder.Services.AddHttpClient(); 

builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddScoped<HttpClient>(sp =>
{
    var navManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navManager.BaseUri) };
});;

#region Autenticação
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth_token";
        options.LoginPath = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Autenticação
app.UseAuthentication();
app.UseAuthorization();

// MAPEIA AS ROTAS DA API
app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// app.UseExceptionHandler("/Error");

app.Run();