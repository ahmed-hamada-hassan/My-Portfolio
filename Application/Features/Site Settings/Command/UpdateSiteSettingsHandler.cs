using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Site_Settings.Command;

internal sealed class UpdateSiteSettingsHandler
    : ICommandHandler<UpdateSiteSettingsCommand, Result>
{
    private readonly ISiteSettingsCommandRepository _siteSettingsCommandRepository;
    private readonly ILogger<UpdateSiteSettingsHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSiteSettingsHandler(
        ISiteSettingsCommandRepository siteSettingsCommandRepository,
        ILogger<UpdateSiteSettingsHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _siteSettingsCommandRepository = siteSettingsCommandRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(
        UpdateSiteSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var siteSettings =
            await _siteSettingsCommandRepository.GetSiteSettingsAsync(
                cancellationToken);

        if (siteSettings is null)
        {
            _logger.LogWarning("Site settings were not found.");

            return Result.Failure(
                SiteSettingsErrors.SiteSettingsNotFound);
        }

        var updateResult = siteSettings.Update(
            command.SiteTitle,
            command.FullName,
            command.JobTitle,
            command.AvailableForWork);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error!);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}