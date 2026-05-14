using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using MyApi.Controllers;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Usles;

public class MainRefreshTokenDealer : IRefreshTokenDealer
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly ILogger<UserController> _logger; 

    public MainRefreshTokenDealer(IConfiguration config, AppDbContext db, ILogger<UserController> logger)
    {
        _config = config;
        _db = db;
        _logger = logger;
    }

    public string GenerateRefreshToken(User user, DTORefreshToken tk)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            ExpireAt = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.AddAsync(refreshToken);
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens.Where(t => t.Token == tk.Token));
        _db.SaveChanges();

        return refreshToken.Token;
    }

    public string GenerateRefreshToken(User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            ExpireAt = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.AddAsync(refreshToken);
        _db.SaveChanges();

        return refreshToken.Token;
    }

}