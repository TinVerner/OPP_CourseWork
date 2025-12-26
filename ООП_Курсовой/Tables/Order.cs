using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ООП_Курсовой.Tables;

[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long? UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Column("status")]
    public OrderStatus Status { get; set; }

    [MaxLength(128)]
    [Column("customer_name")]
    public string? CustomerName { get; set; }
    
    [MaxLength(32)]
    [Column("customer_phone")]
    public string? CustomerPhone { get; set; }

    [Column("delivery_type")]
    public DeliveryType DeliveryType { get; set; }

    [MaxLength(255)]
    [Column("customer_address")]
    public string? CustomerAddress { get; set; }
    
    [Column("delivery_time")]
    public DateTimeOffset? DeliveryTime { get; set; }
    
    [MaxLength(255)]
    [Column("comment")]
    public string? Comment { get; set; }

    [Column("total_price", TypeName = "numeric(10,2)")]
    public decimal TotalPrice { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    
    [Column("delivered_at")]
    public DateTimeOffset? DeliveredAt { get; set; }
}
