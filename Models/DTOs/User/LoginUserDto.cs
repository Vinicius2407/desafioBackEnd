using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.User;
public class LoginUserDto
{
    [Required(ErrorMessage = "O email é obrigatório")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "A senha é obrigatório")]
    public string Password { get; set; } = string.Empty;
}
