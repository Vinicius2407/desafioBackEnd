using Models.DTOs.Transaction;
using Models.DTOs.User;
using Models.Helpers;
using Models.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.DTOs.Bet;
public class CreateBetDto : BaseEntity
{
    [Required(ErrorMessage = "Informe o valor da aposta.")]
    [Range(1, double.MaxValue, ErrorMessage = "O valor da aposta deve ser maior que zero.")]
    public decimal Amount { get; set; }
    [Required(ErrorMessage = "Informe o id do usuario")]
    [Range(1, double.MaxValue, ErrorMessage = "Id do usuario invalido")]
    public long UserId { get; set; }
    [JsonIgnore]
    public List<TransactionViewModel>? Transaction { get; set; } = null;
    [JsonIgnore]
    public new DateTime CreatedAt { get; set; }
}
