using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly AppDbContext _db;
    
    public NoteController(AppDbContext db)
    {
        _db = db;
    }
    
    [HttpPost]
    public IActionResult CreateNote([FromBody] Note newNote)
    {
        if (newNote == null)
            return BadRequest("Нет данных");
        
        if (string.IsNullOrEmpty(newNote.Title))
            return BadRequest("Имя обязательно");
        newNote.CreatedAt = DateTime.Today;
        _db.Notes.Add(newNote);
        _db.SaveChanges();

        return Ok(new 
        { 
            message = $"Заметка {newNote.Title} создана",
            note = newNote 
        });
    }
    
    [HttpGet]
    public IActionResult GetAllNotes()
    {
        return Ok(_db.Notes.ToListAsync());
    }

    [HttpGet("{Id}")]
    public IActionResult GetNoteById(int Id)
    {
        Note? nt = _db.Notes.Find(Id);

        if(nt == null)
            return BadRequest("Note never exist");

        return Ok(nt);
    }

    [HttpPut]
    public IActionResult PutNote([FromBody] Note newNote)
    {
        if (newNote == null)
            return BadRequest("Нет данных");
        
        if (string.IsNullOrEmpty(newNote.Title))
            return BadRequest("Имя обязательно");
        
        Note? nt = _db.Notes.Find(newNote.Id);

        nt?.Title = newNote.Title;
        nt?.Content = newNote.Content;
        
        _db.SaveChanges();

        return Ok(new 
        { 
            message = $"Заметка {newNote.Title} изменена",
            note = newNote 
        });
    }

    [HttpDelete("{Id}")]
    public IActionResult DeleteNote(int Id)
    {
        
        Note? nt = _db.Notes.Find(Id);

        if (nt == null)
            return BadRequest("Заметка не найдена");
        
        _db.Notes.Remove(nt);
        
        _db.SaveChanges();

        return Ok(new 
        { 
            message = $"Заметка под Id {Id} удалена",
        });
    }
}