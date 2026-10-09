using Application.Features.Project_Images.DTOs;
using Application.Features.Project_Images.Validators;
using Application.Options;
using Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Features.Project_Images.Command;

internal sealed class AddProjectImagesValidator
    : AbstractValidator<AddProjectImagesCommand>
{
    public AddProjectImagesValidator(
        IOptionsSnapshot<SiteSettingsOptions> options)
    {
        var imageOptions = options.Value.Images;

        RuleForEach(x => x.ProjectImages)
            .SetValidator(new AddProjectImageDTOValidator(imageOptions));

    }
}