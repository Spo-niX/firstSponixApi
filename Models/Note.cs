using System.Runtime.InteropServices;

namespace MyApi.Models;

public class Note
{
    public int? Id { get; set; }          // ID сам добавится
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? CreatedAt { get; set; }
}