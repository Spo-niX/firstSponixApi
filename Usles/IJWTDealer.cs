using MyApi.Models;

public interface IJWTDealer
{
    public string GenerateJWT(User user); 
}