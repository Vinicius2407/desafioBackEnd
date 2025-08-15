using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Models.Model;
public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [NotMapped]
    [JsonIgnore]
    public List<string> Errors { get; set; } = new List<string>();
    [NotMapped]
    [JsonIgnore]
    public bool HasErrors => Errors.Any();
}
