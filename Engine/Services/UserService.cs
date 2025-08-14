using Engine.Singleton;
using Models.DTOs.User;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Services;
public class UserService
{
    public UserService() { }

    public static UserViewModel SignUp(CreateUserDto createUserDto)
    {
        if (createUserDto == null)
            throw new ArgumentNullException(nameof(createUserDto), "Dados do usuário não podem ser nulos.");

        var passwordHash = Engine.Helpers.PasswordHelper.HashPassword(createUserDto.Password);

        var user = new Models.Model.User
        {
            Id = createUserDto.Id,
            Name = createUserDto.Name,
            Password = passwordHash,
            Email = createUserDto.Email,
            Document = createUserDto.Document,
            PhoneNumber = createUserDto.PhoneNumber
        };

        return SaveUser(user);
    }

    private static UserViewModel SaveUser(User user)
    {
        using var contexto = new DBApp();

        if (user.Id == 0)
            contexto.Users.Add(user);
        else
        {
            var existingUser = contexto.Users.Find(user.Id);

            if (existingUser == null)
                return new UserViewModel
                {
                    Errors = new List<string> { "Usuário não encontrado para atualização." }
                };

            contexto.Entry(existingUser).CurrentValues.SetValues(user);
        }

        contexto.SaveChanges();
        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };

        return userViewModel;
    }
}
