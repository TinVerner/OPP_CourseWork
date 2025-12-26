using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ООП_Курсовой.Tables;

[Table("ingredients_roll")]
public class IngredientRoll
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("roll_id")]
    public long RollId { get; set; }

    [Column("ingredient_id")]
    public long IngredientId { get; set; }

    public MenuItem Roll { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
