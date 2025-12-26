using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ООП_Курсовой.Tables;

[Table("menu_set_components")]
public class MenuSetComponent
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("set_id")]
    public long SetId { get; set; }

    [ForeignKey(nameof(SetId))]
    public MenuItem Set { get; set; } = null!;

    [Column("item_id")]
    public long ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public MenuItem Item { get; set; } = null!;

    [Column("qty")]
    public int Qty { get; set; }
}
