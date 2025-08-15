using Models.DTOs.Transaction;
using Models.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models.DTOs.Bet;
public class CreateBetDto : BaseEntity
{
    [Required(ErrorMessage = "Informe o valor da aposta.")]
    [Range(1, double.MaxValue, ErrorMessage = "O valor da aposta deve ser maior que zero.")]
    public decimal Amount { get; set; }
    [Description("Troque o valor para true se quiser jogar assim que criar a aposta.")]
    public bool AutoPlayOnCreate { get; set; } = false;
    [JsonIgnore]
    public long UserId { get; set; }
    [JsonIgnore]
    public List<TransactionViewModel>? Transaction { get; set; } = null;
    [JsonIgnore]
    public new DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public new long Id { get; set; }
}
