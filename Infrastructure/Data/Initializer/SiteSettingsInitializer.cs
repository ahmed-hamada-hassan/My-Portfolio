using Domain.Entities;
using Infrastructure.Data.EF_Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Initializer;

public static class SiteSettingsInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SiteSettingsInitializer");

        var exists = await dbContext.Set<SiteSettings>()
            .AnyAsync(cancellationToken);

        if (exists)
            return;

        var result = SiteSettings.Create(
            siteTitle: "Ahmed Hamada | Portfolio",
            fullName: "Ahmed Hamada",
            jobTitle: "Junior Full-Stack Developer",
            profileImageUrl: null,
            profileImagePublicId: null,
            cvUrl: null,
            availableForWork: false);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to create default site settings: {result.Error}");
        }

        dbContext.Set<SiteSettings>().Add(result.Value!);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Default site settings were initialized.");
    }
}