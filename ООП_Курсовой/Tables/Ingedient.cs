using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ООП_Курсовой.Tables;

[Table("ingredients")]
public class Ingredient
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("name")]
    public string Name { get; set; } = null!;
    [MaxLength(255)]
    [Column("note")]
    public string? Note { get; set; }
    [Column("is_spicy")]
    public bool IsSpicy { get; set; }
    [Column("is_vegetarian")]
    public bool IsVegetarian { get; set; }

    public ICollection<IngredientRoll> IngredientRolls { get; set; } = new List<IngredientRoll>();
}
