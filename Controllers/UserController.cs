using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;
    
    public UserController(AppDbContext db)
    {
        _db = db;
    }
    
    [HttpPost]
    public IActionResult Create([FromBody] User newUser)
    {
        if (newUser == null)
            return BadRequest("Нет данных");
        
        if (string.IsNullOrEmpty(newUser.Name))
            return BadRequest("Имя обязательно");
        
        // Сохраняем в базу
        _db.Users.Add(newUser);
        _db.SaveChanges();
        
        // Возвращаем результат
        return Ok(new 
        { 
            message = $"Пользователь {newUser.Name} создан",
            user = newUser 
        });
    }
    
    [HttpGet]
    public IActionResult GetAll()
    {
        var users = _db.Users.ToListAsync();
        return Ok(users);
    }
}