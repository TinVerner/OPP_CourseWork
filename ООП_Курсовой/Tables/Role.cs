using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ООП_Курсовой.Tables;

[Table("roles")]
public class Role
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(64)]
    [Column("name")]
    public string Name { get; set; } = null!;
}
