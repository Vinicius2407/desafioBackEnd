using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static Models.Helpers.Enumerators;

namespace Models.DTOs.Transaction;
public class CreateTransactionDto
{
    [JsonIgnore]
    public long TransactionId { get; set; }
    [Required(ErrorMessage = "Necessita informar a quantidade que quer retirar ou depositar.")]
    [Range(1, double.MaxValue, ErrorMessage = "O valor deve ser maior que 1.")]
    public decimal Amount { get; set; }
    [Required(ErrorMessage = "Necessita informar o tipo de transação.")]
    [EnumDataType(typeof(TransactionType), ErrorMessage = "Tipo de transação inválida")]
    public TransactionType Type { get; set; }
    [JsonIgnore]
    public long UserId { get; set; }
}
