using Domain.Constants;
using FluentValidation;

namespace Application.Features.Site_Settings.Command;

internal sealed class UpdateSiteSettingsValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    public UpdateSiteSettingsValidator()
    {
        RuleFor(x => x.SiteTitle)
            .NotEmpty()
            .When(x => x.SiteTitle is not null)
            .MaximumLength(MaxLength.SiteSettingsSiteTitle)
            .When(x => x.SiteTitle is not null);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .When(x => x.FullName is not null)
            .MaximumLength(MaxLength.SiteSettingsFullName)
            .When(x => x.FullName is not null);

        RuleFor(x => x.JobTitle)
            .NotEmpty()
            .When(x => x.JobTitle is not null)
            .MaximumLength(MaxLength.SiteSettingsJobTitle)
            .When(x => x.JobTitle is not null);
    }
}
