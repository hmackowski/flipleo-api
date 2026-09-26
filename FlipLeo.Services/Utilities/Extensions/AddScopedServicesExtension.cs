using FlipLeo.Core.Interfaces;
using FlipLeo.Repository;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FlipLeo.Services.Utilities.Extensions;

public static class AddScopedServicesExtension
{
    public static void AddScopedServices(this IServiceCollection services)
    {
        // Unit of Work
        services.AddScoped<IFlipLeoUnitOfWork, FlipLeoUnitOfWork>();

        // Current user (placeholder until authentication is added)
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Services
        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IFlipRecordService, FlipRecordService>();
        services.AddScoped<IGreetingService, GreetingService>();
        services.AddScoped<ILookupService, LookupService>();
    }
}
