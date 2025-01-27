using MongoDB.Driver;
using Service.Models;
using System.Security.Cryptography;
using System.Text;

namespace Service.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    public async Task InitializeSeedUserAsync()
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, "admin");
        var cursor = await _users.FindAsync(filter);
        var userExists = await cursor.AnyAsync();
        
        if (!userExists)
        {
            var hashedPassword = HashPassword("sk2025");
            var seedUser = new User
            {
                Username = "admin",
                Password = hashedPassword
            };
            await _users.InsertOneAsync(seedUser);
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var hashedPassword = HashPassword(password);
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Username, username),
            Builders<User>.Filter.Eq(u => u.Password, hashedPassword)
        );
        var cursor = await _users.FindAsync(filter);
        return await cursor.AnyAsync();
    }
} 
