using System.Security.Cryptography;
using System.Text;
using FlipLeo.Core.Constants;
using FlipLeo.Core.DTOs.Auth;
using FlipLeo.Core.Exceptions;
using FlipLeo.Core.Interfaces;
using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlipLeo.Services;

public class AuthService : IAuthService
{
    private const string InvalidLoginMessage = "Invalid email or password.";
    private const string InvalidResetLinkMessage = "This reset link is invalid or has expired. Please request a new one.";
    private static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromHours(1);

    private readonly IFlipLeoUnitOfWork _flipLeoUnitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccountEmailService _accountEmailService;

    // Used when the email doesn't exist, so a failed login takes the same time either way
    // (otherwise response time would reveal which emails have accounts)
    private static string? _dummyHash;

    public AuthService(
        IFlipLeoUnitOfWork flipLeoUnitOfWork,
        IPasswordHasher passwordHasher,
        IAccountEmailService accountEmailService)
    {
        _flipLeoUnitOfWork = flipLeoUnitOfWork ?? throw new ArgumentNullException(nameof(flipLeoUnitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _accountEmailService = accountEmailService ?? throw new ArgumentNullException(nameof(accountEmailService));
    }

    public async Task<UserProfile> Register(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);

        var emailTaken = await _flipLeoUnitOfWork.UserAccountRepository.AnyAsync(u => u.Email == email);
        if (emailTaken)
            throw new ConflictException("An account with this email already exists.");

        var now = DateTime.UtcNow;
        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            LastLoginDate = now,
            CreatedDate = now,
            UpdatedDate = now
        };

        _flipLeoUnitOfWork.UserAccountRepository.Add(user);
        await _flipLeoUnitOfWork.CommitAsync();

        return ToProfile(user);
    }

    public async Task<UserProfile> Login(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _flipLeoUnitOfWork.UserAccountRepository.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null || !user.IsActive)
        {
            _passwordHasher.Verify(request.Password, _dummyHash ??= _passwordHasher.Hash("not-a-real-password"));
            throw new UnauthorizedException(InvalidLoginMessage);
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException(InvalidLoginMessage);

        user.LastLoginDate = DateTime.UtcNow;
        user.UpdatedDate = DateTime.UtcNow;
        await _flipLeoUnitOfWork.CommitAsync();

        return ToProfile(user);
    }

    public async Task<UserProfile> GetUser(Guid userId)
    {
        var user = await _flipLeoUnitOfWork.UserAccountRepository
            .Find(u => u.Id == userId && u.IsActive)
            .Select(u => new UserProfile { Id = u.Id, Email = u.Email, DisplayName = u.DisplayName })
            .SingleOrDefaultAsync();

        return user ?? throw new UnauthorizedException("Your account could not be found.");
    }

    public async Task ForgotPassword(ForgotPasswordRequest request)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _flipLeoUnitOfWork.UserAccountRepository
            .SingleOrDefaultAsync(u => u.Email == email && u.IsActive);

        // No account: do nothing, but look exactly the same to the caller
        if (user is null)
            return;

        var now = DateTime.UtcNow;

        // Only the newest link works: retire any earlier unused ones
        var openTokens = await _flipLeoUnitOfWork.UserAccountTokenRepository
            .Find(t => t.UserAccountId == user.Id && t.Purpose == UserAccountTokenPurposes.PasswordReset && t.UsedDate == null)
            .ToListAsync();
        foreach (var openToken in openTokens)
            openToken.UsedDate = now;

        var token = CreateToken();
        _flipLeoUnitOfWork.UserAccountTokenRepository.Add(new UserAccountToken
        {
            UserAccountId = user.Id,
            Purpose = UserAccountTokenPurposes.PasswordReset,
            TokenHash = HashToken(token),
            ExpiresDate = now.Add(PasswordResetLifetime),
            CreatedDate = now
        });
        await _flipLeoUnitOfWork.CommitAsync();

        // The plain token only ever exists in the email link
        await _accountEmailService.SendPasswordResetAsync(user.Email, user.DisplayName, token);
    }

    public async Task ResetPassword(ResetPasswordRequest request)
    {
        var tokenHash = HashToken(request.Token.Trim());
        var now = DateTime.UtcNow;

        var resetToken = await _flipLeoUnitOfWork.UserAccountTokenRepository
            .Find(t => t.TokenHash == tokenHash
                       && t.Purpose == UserAccountTokenPurposes.PasswordReset
                       && t.UsedDate == null
                       && t.ExpiresDate > now)
            .Include(t => t.UserAccount)
            .SingleOrDefaultAsync();

        if (resetToken is null || !resetToken.UserAccount.IsActive)
            throw new BadRequestException(InvalidResetLinkMessage);

        var user = resetToken.UserAccount;
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedDate = now;
        resetToken.UsedDate = now; // one use only

        await _flipLeoUnitOfWork.CommitAsync();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>256 random bits as hex: URL-safe and impossible to guess.</summary>
    private static string CreateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static UserProfile ToProfile(UserAccount user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName
    };
}
