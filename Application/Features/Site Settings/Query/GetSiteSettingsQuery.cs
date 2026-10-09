using Application.Features.Site_Settings.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Site_Settings.Query;

public record GetSiteSettingsQuery() : IQuery<Result<SiteSettingsResponse>>;