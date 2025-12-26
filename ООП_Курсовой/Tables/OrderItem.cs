using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ООП_Курсовой.Tables;

[Table("order_items")]
public class OrderItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("order_id")]
    public long OrderId { get; set; }
    
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    [Column("menu_item_id")]
    public long MenuItemId { get; set; }
    
    [ForeignKey(nameof(MenuItemId))]
    public MenuItem MenuItem { get; set; } = null!;

    [Column("qty")]
    public int Qty { get; set; }

    [Column("price", TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }
}
