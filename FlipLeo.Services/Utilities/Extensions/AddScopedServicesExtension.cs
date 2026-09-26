using FlipLeo.Repository;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FlipLeo.Services.Utilities.Extensions;

public static class AddScopedServicesExtension
{
    /// <remarks>
    /// ICurrentUserService is registered by the Api project, because it reads the user from the HTTP request.
    /// </remarks>
    public static void AddScopedServices(this IServiceCollection services)
    {
        // Unit of Work
        services.AddScoped<IFlipLeoUnitOfWork, FlipLeoUnitOfWork>();

        // Utilities (stateless, so one shared instance is fine)
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Services
        services.AddScoped<IAddOnPresetService, AddOnPresetService>();
        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFlipRecordService, FlipRecordService>();
        services.AddScoped<ILookupService, LookupService>();
    }
}
