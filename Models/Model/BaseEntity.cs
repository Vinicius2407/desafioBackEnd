using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Model;
public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [NotMapped]
    public List<string> Errors { get; set; } = new List<string>();
    [NotMapped]
    public bool HasErrors => Errors.Any();
}
