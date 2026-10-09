using Application.Features.Site_Settings.DTOs;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;

namespace Application.Features.Site_Settings.Query;

internal sealed class GetSiteSettingsHandler : IQueryHandler<GetSiteSettingsQuery, Result<SiteSettingsResponse>>
{
    private readonly ISiteSettingsQueryRepository _siteSettingsQueryRepository;

    public GetSiteSettingsHandler(ISiteSettingsQueryRepository siteSettingsQueryRepository)
    {
        _siteSettingsQueryRepository = siteSettingsQueryRepository;
    }

    public async ValueTask<Result<SiteSettingsResponse>> Handle(GetSiteSettingsQuery query, CancellationToken cancellationToken)
    {
        var siteSettings = await _siteSettingsQueryRepository.GetSiteSettingsAsync(cancellationToken);
        if(siteSettings is null)
            return Result<SiteSettingsResponse>.Failure(SiteSettingsErrors.SiteSettingsNotFound);

        return Result<SiteSettingsResponse>.Success(siteSettings);
    }
}
