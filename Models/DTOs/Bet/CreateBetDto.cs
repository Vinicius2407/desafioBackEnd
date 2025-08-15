using Models.DTOs.Transaction;
using Models.Helpers;
using Models.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Bet;
public class CreateBetDto : BaseEntity
{
    [Required(ErrorMessage = "Informe o valor da aposta.")]
    [Range(1, double.MaxValue, ErrorMessage = "O valor da aposta deve ser maior que zero.")]
    public decimal Amount { get; set; }
    public long UserId { get; set; }
    public Enumerators.BetStatus Status { get; set; } = Enumerators.BetStatus.PENDING;
    public List<TransactionViewModel>? Transaction { get; set; } = null;
}
