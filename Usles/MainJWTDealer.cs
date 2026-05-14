using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.IdentityModel.Tokens;
using MyApi.Controllers;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Usles;

public class MainJWTDealer : IJWTDealer
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly ILogger<UserController> _logger; 

    public MainJWTDealer(IConfiguration config, AppDbContext db, ILogger<UserController> logger)
    {
        _config = config;
        _db = db;
        _logger = logger;
    }

    public string GenerateJWT(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)

        };
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["SecretKey"] ?? "supersecretkey"));
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha512)
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _db.SaveChanges();

        _logger.LogInformation("User {id} loginned with token {token}", user.Id, tokenString);

        return tokenString;
    }

}