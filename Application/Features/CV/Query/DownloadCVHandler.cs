using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.CV.Query;

internal sealed class DownloadCVHandler : IQueryHandler<DownloadCVQuery, Result<Stream>>
{
    private readonly ISiteSettingsQueryRepository _siteSettingsQueryRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly ILogger<DownloadCVHandler> _logger;

    public DownloadCVHandler(ISiteSettingsQueryRepository siteSettingsQueryRepository, IDocumentStorageService documentStorageService, 
        ILogger<DownloadCVHandler> logger)
    {
        _siteSettingsQueryRepository = siteSettingsQueryRepository;
        _documentStorageService = documentStorageService;
        _logger = logger;
    }

    public async ValueTask<Result<Stream>> Handle(DownloadCVQuery query, CancellationToken cancellationToken)
    {
        var siteSettings = await _siteSettingsQueryRepository.GetSiteSettingsAsync(cancellationToken);
        if(siteSettings is null)
        {
            _logger.LogWarning("Site settings were not found.");

            return Result<Stream>.Failure(
                SiteSettingsErrors.SiteSettingsNotFound);
        }

        var cvUrl = siteSettings.CVURL;
        if(string.IsNullOrWhiteSpace(cvUrl))
        {
            _logger.LogWarning("CV URL is not set in site settings.");
            return Result<Stream>.Failure(
                SiteSettingsErrors.CVNotFound);
        }

        var cvStream = await _documentStorageService.OpenReadAsync(cvUrl, cancellationToken);
        if(cvStream.IsFailure)
        {
            _logger.LogWarning("Failed to open CV from storage. Error: {Error}", cvStream.Error);
            return Result<Stream>.Failure(cvStream.Error!);
        }

        return Result<Stream>.Success(cvStream.Value!);
    }
}
