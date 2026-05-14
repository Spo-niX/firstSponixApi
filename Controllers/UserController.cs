using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyApi.Data;
using MyApi.Usles;
using MyApi.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using System.Security.Cryptography;
using System.Net.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserController> _logger; 
    private readonly IConfiguration _config;
    private readonly IRefreshTokenDealer _rf;
    private readonly IJWTDealer _jwt;
    public UserController(AppDbContext db, ILogger<UserController> logger, IConfiguration config, IRefreshTokenDealer rf, IJWTDealer jwt)  
    {
        _db = db;
        _logger = logger;
        _config = config;
        _rf = rf;
        _jwt = jwt;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] DTOUser us)
    {
        if(string.IsNullOrWhiteSpace(us.Name) || string.IsNullOrWhiteSpace(us.Password))
        {
            return BadRequest("Please, enter correct data");
        }
        
        if(await _db.Users.FirstOrDefaultAsync(x => x.Email == us.Email) != null)
        {
            return BadRequest("User with that email already registered");
        }

        User endUser = new User();
        endUser.Name = us.Name;
        endUser.Email = us.Email;
        endUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(us.Password);
        endUser.Role = "User";

        _db.Users.Add(endUser);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {name} registered", us.Name);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LogInUser([FromBody] LogInUser us)
    {
        if(string.IsNullOrWhiteSpace(us.Email) || string.IsNullOrWhiteSpace(us.Password))
        {
            return BadRequest("Please, enter correct data");
        }
        
        var tryUs = await _db.Users.FirstOrDefaultAsync(u => u.Email == us.Email);

        if(tryUs == null)
        {
            return BadRequest("No users with that Email");
        }

        if(!BCrypt.Net.BCrypt.Verify(us.Password, tryUs.PasswordHash))
        {
            return BadRequest("Wrong password");
        }

        string accessToken = _jwt.GenerateJWT(tryUs);
        string refreshToken = _rf.GenerateRefreshToken(tryUs);

        _logger.LogInformation("User {id} loginned with refresh token {token}", tryUs.Id, refreshToken);

        return Ok(new
        {
            accessToken = accessToken,
            refreshToken = refreshToken
        });
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh([FromBody] DTORefreshToken tk)
    {
        if(string.IsNullOrWhiteSpace(tk.Token))
        {
            return BadRequest("Please, enter correct data");
        }
        
        var t = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tk.Token);
        if(t == null) return Unauthorized("Wrong token");
        if(t.ExpireAt < DateTime.UtcNow) return Unauthorized("token has been expired");
        var Us = await _db.Users.FirstOrDefaultAsync(u => u.Id == t.UserId);

        string accessToken = _jwt.GenerateJWT(Us);
        string refreshToken = _rf.GenerateRefreshToken(Us, tk);

        _logger.LogInformation("User {id} loginned with refresh token {token}", Us.Id, refreshToken);

        return Ok(new
        {
            accessToken = accessToken,
            refreshToken = refreshToken
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> getMe()
    {
        return Ok(User.Identity?.Name);
    }
}