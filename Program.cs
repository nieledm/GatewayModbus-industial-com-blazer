using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using DL6000WebConfig.Data;
using DL6000WebConfig.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ModbusVariableService>(sp =>
{
    var configService = sp.GetRequiredService<ConfigService>();
    return new ModbusVariableService(Path.Combine("variables.json"), configService);
});
// "..",
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

//adicionando o caminho do arquivo de configurção do DL6000
builder.Services.AddSingleton<ConfigService>(sp =>
    new ConfigService(Path.Combine("..", "DL6000_TO_MODBUS_SLAVE.exe.config")));

#region Autenticação
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth_token";
        options.LoginPath = "/login";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<UserService>();
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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// MAPEIA AS ROTAS DA API
app.MapControllers();

app.Run();

