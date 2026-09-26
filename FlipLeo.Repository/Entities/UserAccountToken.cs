namespace FlipLeo.Repository.Entities;

/// <summary>A one-time emailed token (e.g. password reset). Only the hash is stored.</summary>
public class UserAccountToken
{
    public int Id { get; set; }

    public Guid UserAccountId { get; set; }

    /// <summary>See UserAccountTokenPurposes.</summary>
    public string Purpose { get; set; } = null!;

    /// <summary>Hex SHA-256 of the token in the emailed link.</summary>
    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresDate { get; set; }

    public DateTime? UsedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual UserAccount UserAccount { get; set; } = null!;
}
