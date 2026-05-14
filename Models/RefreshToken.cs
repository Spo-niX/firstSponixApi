using System.Runtime.InteropServices;

namespace MyApi.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpireAt { get; set; }
}