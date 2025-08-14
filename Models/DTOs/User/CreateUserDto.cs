using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.User;
public class CreateUserDto
{
    public long Id { get; set; } = 0;

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatório")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Documento é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 20 caracteres")]
    public string Document { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numero de telefone é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 15 caracteres")]
    public string PhoneNumber { get; set; } = string.Empty;
}
