namespace ООП_Курсовой.SupportClasses;

public static class CurrentUser
{
    public static long Id { get; set; }

    public static string Login { get; set; } = string.Empty;

    public static string Email { get; set; } = string.Empty;

    public static string Phone { get; set; } = string.Empty;

    public static string Firstname { get; set; } = string.Empty;

    public static string Lastname { get; set; } = string.Empty;

    public static string FullName => $"{Firstname} {Lastname}".Trim();

    public static string RoleName { get; set; } = string.Empty;

    public static int RoleId { get; set; }

    public static bool IsBlocked { get; set; }
    public static string Address { get; set; } = string.Empty;

    public static DateTimeOffset CreatedAt { get; set; }

    public static bool IsAuthenticated => Id > 0;

    public static void Logout()
    {
        Id = 0;
        Login = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Firstname = string.Empty;
        Lastname = string.Empty;
        RoleName = string.Empty;
        RoleId = 0;
        IsBlocked = false;
        CreatedAt = default;
    }
}