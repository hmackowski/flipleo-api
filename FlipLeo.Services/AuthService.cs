using FlipLeo.Core.DTOs.Auth;
using FlipLeo.Core.Exceptions;
using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlipLeo.Services;

public class AuthService : IAuthService
{
    private const string InvalidLoginMessage = "Invalid email or password.";

    private readonly IFlipLeoUnitOfWork _flipLeoUnitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    // Used when the email doesn't exist, so a failed login takes the same time either way
    // (otherwise response time would reveal which emails have accounts)
    private static string? _dummyHash;

    public AuthService(IFlipLeoUnitOfWork flipLeoUnitOfWork, IPasswordHasher passwordHasher)
    {
        _flipLeoUnitOfWork = flipLeoUnitOfWork ?? throw new ArgumentNullException(nameof(flipLeoUnitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static UserProfile ToProfile(UserAccount user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName
    };
}
