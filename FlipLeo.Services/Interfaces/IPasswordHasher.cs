namespace FlipLeo.Services.Interfaces;

public interface IPasswordHasher
{
    /// <summary>Returns a salted hash safe to store in UserAccount.PasswordHash.</summary>
    string Hash(string password);

    /// <summary>True if the password matches a hash produced by Hash().</summary>
    bool Verify(string password, string passwordHash);
}
