using Echos.Api.Domain.Users;
using Echos.Api.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Echos.Api.Application.Users;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User> ExecuteAsync(
        string userName,
        string name,
        string email,
        string password)
    {
        // Valida formato
        CreateUserValidator.Validate(email, password);

        // Normaliza
        userName = userName.Trim().ToLowerInvariant();
        email = email.Trim().ToLowerInvariant();
        name = name.Trim();

        // checa duplicidade
        var exists = await _db.Users.AnyAsync(u =>
            !u.IsDeleted &&
            (u.UserName == userName || u.Email == email)
        );

        if (exists)
            throw new InvalidOperationException("Username or Email already in use.");

        // criptografa senha usando BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);


        var user = new User(
            userName,
            name,
            email,
            passwordHash
        );

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }
}
