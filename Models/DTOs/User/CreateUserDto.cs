using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.User;
public class CreateUserDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatório")]
    [Range(6, 100, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Documento é obrigatório")]
    [StringLength(20, ErrorMessage = "O nome deve ter no máximo 20 caracteres")]
    public string Document { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numero de telefone é obrigatório")]
    [StringLength(15, ErrorMessage = "O nome deve ter no máximo 15 caracteres")]
    public string PhoneNumber { get; set; } = string.Empty;
}
