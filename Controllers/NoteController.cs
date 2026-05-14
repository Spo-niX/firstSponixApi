using Microsoft.AspNetCore.Authorization;
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
    private readonly ILogger<NoteController> _logger;
    
    IAsyncEnumerable<Note> notes;  
    
    public NoteController(AppDbContext db, ILogger<NoteController> logger)  // ← добавить
    {
        _db = db;
        _logger = logger;
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateNote([FromBody] DTONote newNote)
    {
        if (newNote == null)
            return BadRequest("Нет данных");
        if (string.IsNullOrEmpty(newNote.Title))
            return BadRequest("Имя обязательно");

        Note finNote = new Note();
        var U = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if(U == null) return Unauthorized();
        finNote.UserId = U.Id;
        finNote.Title = newNote.Title;
        finNote.Content = newNote.Content;
        finNote.CreatedAt = DateTime.Now;
        _db.Notes.Add(finNote);
        _db.SaveChanges();

        _logger.LogInformation("Создание {0} c {1}", newNote.Title, finNote.Id);

        return Ok(new 
        { 
            message = $"Заметка {newNote.Title} создана",
            note = newNote 
        });
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllNotes()
    {
        var U = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if(U == null) return Unauthorized();
        if (User.IsInRole("Admin"))
        {
            var notes = await _db.Notes.Where(u => u.UserId == U.Id).ToListAsync();
            return Ok(notes);
        }
        else
        {
            var notes = await _db.Notes.Where(u => u.UserId == U.Id).ToListAsync();
            return Ok(notes);
        }
    }

    [HttpGet("{Id}")]
    [Authorize]
    public async Task<IActionResult> GetNoteById(int Id)
    {
        var U = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if(U == null) return Unauthorized();

        Note? nt = _db.Notes.Find(Id);

        if(nt == null)
            return NotFound("Note never exist");

        if(nt.UserId != U.Id && !User.IsInRole("Admin")) return Unauthorized();

        return Ok(nt);
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchNotes(
        [FromQuery] string? search,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var U = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if(U == null) return Unauthorized();


        List<Note> res = new List<Note>();
        
        if(page != 0 && pageSize != 0)
        {
            int actPage = page ?? 1;
            int actSize = pageSize ?? 10;

            res.AddRange(await _db.Notes.Where(u => u.UserId == U.Id).AsQueryable().Skip((actPage - 1)*actSize).Take(actSize).ToListAsync());
        }
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            if (User.IsInRole("Admin"))
            {
                var notes = _db.Notes.AsAsyncEnumerable();
            }
            else
            {
                var notes = _db.Notes.Where(u => u.UserId == U.Id).AsAsyncEnumerable();
            }
            
            var p = notes.Where(x => x.Title.ToUpper().Contains(search.ToUpper()) || x.Content.ToUpper().Contains(search.ToUpper()));

            res.AddRange(await p.ToListAsync());
        }
        
        if(from != null)
        {
            if (User.IsInRole("Admin"))
            {
                var notes = _db.Notes.AsAsyncEnumerable();
            }
            else
            {
                var notes = _db.Notes.Where(u => u.UserId == U.Id).AsAsyncEnumerable();
            }
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
            if (User.IsInRole("Admin"))
            {
                var notes = _db.Notes.AsAsyncEnumerable();
            }
            else
            {
                var notes = _db.Notes.Where(u => u.UserId == U.Id).AsAsyncEnumerable();
            }
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

    [HttpPut("{Id}")]
    [Authorize]
    public async Task<IActionResult> PutNote(int Id, [FromBody] DTONote newNote)
    {
        var U = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if(U == null) return Unauthorized();

        if (newNote == null)
            return BadRequest("Нет данных");
        
        if (string.IsNullOrEmpty(newNote.Title))
            return BadRequest("Имя обязательно");
        
        Note? nt = _db.Notes.Find(Id);

        if(nt.UserId != U.Id && !User.IsInRole("Admin")) return Unauthorized();

        nt?.Title = newNote.Title;
        nt?.Content = newNote.Content;
        
        _db.SaveChanges();

        _logger.LogInformation("Изменение {0} c {1}", newNote.Title, nt.Id);

        return Ok(new 
        { 
            message = $"Заметка {newNote.Title} изменена"
        });
    }

    [HttpDelete("{Id}")]
    [Authorize]
    public async Task<IActionResult> DeleteNote(int Id)
    {
        var U = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if(U == null) return Unauthorized();
        
        Note? nt = _db.Notes.Find(Id);

        if (nt == null)
            return  NotFound("Note never exist");

        if(nt.UserId != U.Id && !User.IsInRole("Admin")) return Unauthorized();
        
        _db.Notes.Remove(nt);
        
        _db.SaveChanges();

        _logger.LogInformation("Удаление {0} c {1}", nt.Title, Id);

        return Ok(new 
        { 
            message = $"Заметка под Id {Id} удалена",
        });
    }
}