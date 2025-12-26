namespace ООП_Курсовой.SupportClasses;

public class RollViewModel
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public long? CategoryId { get; set; }
    public List<long> IngredientIds { get; set; } = new();
    public List<string> IngredientNames { get; set; } = new();

    public string IngredientsText => IngredientNames != null
        ? string.Join(", ", IngredientNames)
        : string.Empty;

    public string? Description { get; set; }

    public decimal? ProteinG { get; set; }
    public decimal? FatG { get; set; }
    public decimal? CarbsG { get; set; }
    public decimal? CaloriesKcal { get; set; }

    public string? ItemTypeDisplay { get; set; }
    public string? ItemType { get; set; }

}