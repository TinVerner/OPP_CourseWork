namespace ООП_Курсовой.SupportClasses;

public class MenuItemDisplay
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string ItemType { get; set; } = "";
    public string ItemTypeDisplay { get; set; } = "";
    public string? Description { get; set; }
    public string ShortDescription { get; set; } = "";
    public string? ImageUrl { get; set; }
}