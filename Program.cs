using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyApi.Data;
using MyApi.Usles;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// База данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// JWT авторизация (если используешь)
var secretKey = builder.Configuration["SecretKey"] ?? "supersecretkey";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    });

builder.Services.AddAuthorization();

builder.Services.AddLogging();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

builder.Services.AddScoped<IJWTDealer, MainJWTDealer>();
builder.Services.AddScoped<IRefreshTokenDealer, MainRefreshTokenDealer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http:/localhost:5137")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

app.UseCors("AllowFrontend");

app.MapGet("/login-google", () => Results.Challenge(
    new AuthenticationProperties { RedirectUri = "/auth/callback" },
    authenticationSchemes: new List<string> { GoogleDefaults.AuthenticationScheme }
));

using ( var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    app.MapGet("/auth/callback", async (HttpContext context) =>
    {
        var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded) return Results.BadRequest();

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;


        return Results.Ok(new { email, name });
    });
}

app.Run();