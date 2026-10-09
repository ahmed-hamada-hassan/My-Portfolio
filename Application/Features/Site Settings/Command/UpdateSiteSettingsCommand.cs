using Domain.Shared;
using Mediator;

namespace Application.Features.Site_Settings.Command;

public record UpdateSiteSettingsCommand(string? SiteTitle, string? FullName, string? JobTitle, bool? AvailableForWork) : ICommand<Result>;