namespace FlipLeo.Repository.Entities;

public class UserAccount
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    /// <summary>Salted PBKDF2 hash produced by IPasswordHasher. Never the plain password.</summary>
    public string PasswordHash { get; set; } = null!;

    public DateTime? LastLoginDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
}
