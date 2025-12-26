using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ООП_Курсовой.Tables;

[Table("categories")]
public class Category
{
    [Key]
    [Column("id")] 
    public long Id { get; set; }
    [Column("parent_id")]
    public long? ParentId { get; set; }
    [Column("name")]
    [Required, MaxLength(128)] 
    public string Name { get; set; } = null!;
}