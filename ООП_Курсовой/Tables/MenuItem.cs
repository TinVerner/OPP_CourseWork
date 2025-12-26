using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ООП_Курсовой.Tables
{
    [Table("menu_items")]
    public class MenuItem
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("category_id")]
        public long? CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("item_type")]
        public string ItemType { get; set; } = null!;

        [Required, MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("price", TypeName = "numeric(10,2)")]
        public decimal Price { get; set; }

        [Column("protein_g", TypeName = "numeric(6,2)")]
        public decimal? ProteinG { get; set; }

        [Column("fat_g", TypeName = "numeric(6,2)")]
        public decimal? FatG { get; set; }

        [Column("carbs_g", TypeName = "numeric(6,2)")]
        public decimal? CarbsG { get; set; }

        [Column("calories_kcal", TypeName = "numeric(7,2)")]
        public decimal? CaloriesKcal { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<IngredientRoll> IngredientRolls { get; set; } = [];
        [InverseProperty(nameof(MenuSetComponent.Set))]
        public ICollection<MenuSetComponent> SetComponents { get; set; } = [];
        [NotMapped]
        public ICollection<MenuSetComponent> IncludedInSets { get; set; } = [];


    }
}
   