using Microsoft.AspNetCore.Http.HttpResults;
using MyApi.Models;

public interface IRefreshTokenDealer
{
    public string GenerateRefreshToken(User user, DTORefreshToken tk); 
    public string GenerateRefreshToken(User user); 
}