using System.Runtime.InteropServices;

namespace MyApi.Models;

public class Note
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
}