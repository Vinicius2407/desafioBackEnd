using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.DTOs.User;
using Models.Model;

namespace Engine.Services;
public class UserService : IService
{
    private readonly DBApp _context;
    public UserService(DBApp context)
    {
        _context = context;
    }

    public async Task<UserViewModel> SignUp(CreateUserDto createUserDto)
    {
        if (createUserDto == null)
            throw new ArgumentNullException(nameof(createUserDto), "Dados do usuário não podem ser nulos.");

        if (GetUserByEmail(createUserDto.Email).Result != null)
            return new UserViewModel { Errors = new List<string> { "Email já em uso!" } };

        var passwordHash = Engine.Helpers.PasswordHelper.HashPassword(createUserDto.Password);

        var user = new User
        {
            Name = createUserDto.Name,
            Password = passwordHash,
            Email = createUserDto.Email,
            Document = createUserDto.Document,
            PhoneNumber = createUserDto.PhoneNumber
        };

        return await SaveUser(user);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => string.Equals(x.Email, email));
    }

    private async Task<UserViewModel> SaveUser(User user)
    {

        if (user.Id == 0)
            _context.Users.Add(user);
        else
        {
            var existingUser = await _context.Users.FindAsync(user.Id);

            if (existingUser == null)
                return new UserViewModel
                {
                    Errors = new List<string> { "Usuário não encontrado para atualização." }
                };

            _context.Entry(existingUser).CurrentValues.SetValues(user);
        }

        await _context.SaveChangesAsync();
        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };

        return userViewModel;
    }
}
