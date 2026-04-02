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
    public async Task<IActionResult> GetAllNotes()
    {
        var notes = await _db.Notes.ToListAsync();
        return Ok(notes);
    }

    [HttpGet("{Id}")]
    public IActionResult GetNoteById(int Id)
    {
        Note? nt = _db.Notes.Find(Id);

        if(nt == null)
            return NotFound("Note never exist");

        return Ok(nt);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchNotes(
        [FromQuery] string? search,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        List<Note> res = new List<Note>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var notes = _db.Notes.AsAsyncEnumerable();

            var p = notes.Where(x => x.Title.ToUpper().Contains(search.ToUpper()) || x.Content.ToUpper().Contains(search.ToUpper()));

            res.AddRange(await p.ToListAsync());
        }
        if(from != null)
        {
            var notes = _db.Notes.AsAsyncEnumerable();
            if(notes == null) return NotFound("No any notes exist");
            await foreach(Note nt in notes)
            {
                if(nt.CreatedAt > from)
                {
                    res.Add(nt);
                }
            }
        }
        if(to != null)
        {
            var notes = _db.Notes.AsAsyncEnumerable();
            if(notes == null) return NotFound("No any notes exist");
            await foreach(Note nt in notes)
            {
                if(nt.CreatedAt < to)
                {
                    res.Add(nt);
                }
            }
        }

        if(res.Count != 0)
            return Ok(res);

        return NotFound("No any notes found");
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
            return  NotFound("Note never exist");
        
        _db.Notes.Remove(nt);
        
        _db.SaveChanges();

        return Ok(new 
        { 
            message = $"Заметка под Id {Id} удалена",
        });
    }
}