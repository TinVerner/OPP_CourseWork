using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ООП_Курсовой.Tables;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required, MaxLength(100)]
    [Column("login")]
    public string Login { get; set; } = null!;

    [Required, MaxLength(32)]
    [Column("phone")]
    public string Phone { get; set; } = null!;

    [Required, MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = null!;

    [Required, MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Required, MaxLength(100)]
    [Column("firstname")]
    public string Firstname { get; set; } = null!;

    [Required, MaxLength(100)]
    [Column("lastname")]
    public string Lastname { get; set; } = null!;

    [MaxLength(255)]
    [Column("address")]
    public string? Address { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("is_blocked")]
    public bool IsBlocked { get; set; }

    [ForeignKey(nameof(Role))]
    [Column("role_id")]
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;
}