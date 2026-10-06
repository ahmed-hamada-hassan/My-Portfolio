using Application.Features.Project.DTOs;
using Application.Features.Project_Images.DTOs;
using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Project.Command;

internal sealed class CreateProjectHandler
    : ICommandHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IProjectCommandRepository _projectCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ISkillCommandRepository _skillCommandRepository;
    private readonly ILogger<CreateProjectHandler> _logger;

    public CreateProjectHandler(
        IProjectCommandRepository projectCommandRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ISkillCommandRepository skillCommandRepository,
        ILogger<CreateProjectHandler> logger)
    {
        _projectCommandRepository = projectCommandRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _skillCommandRepository = skillCommandRepository;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.CreateProjectRequest;

        var titleResult = await EnsureTitleIsUnique(
            request.Title,
            cancellationToken);

        if (titleResult.IsFailure)
        {
            _logger.LogWarning(
                "Attempted to create Project with an existing title.");

            return Result<Guid>.Failure(titleResult.Error!);
        }

        var skillsExist = await EnsureAllSkillsExist(request.SkillIds, cancellationToken);

        if (skillsExist.IsFailure)
        {
            _logger.LogWarning(
                "Attempted to create Project with one or more non-existent Skills.");

            return Result<Guid>.Failure(skillsExist.Error!);
        }

        var projectResult = CreateProject(request);

        if (projectResult.IsFailure)
        {
            _logger.LogWarning(
                "Project creation failed due to domain validation. ErrorCode: {ErrorCode}",
                projectResult.Error!.Code);

            return Result<Guid>.Failure(projectResult.Error!);
        }

        var project = projectResult.Value!;

        var uploadedImages = new List<(string Url, string PublicId)>();

        var imagesResult = await AddProjectImages(
            project,
            request.ProjectImages,
            uploadedImages,
            cancellationToken);

        if (imagesResult.IsFailure)
            return Result<Guid>.Failure(imagesResult.Error!);

        try
        {
            await _projectCommandRepository.AddAsync(
                project,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            await CleanupUploadedImages(uploadedImages);

            throw;
        }

        _logger.LogInformation(
            "Project created successfully. ProjectId: {ProjectId}",
            project.Id);

        return Result<Guid>.Success(project.Id);
    }

    private async Task<Result> EnsureTitleIsUnique(string title, CancellationToken cancellationToken)
    {
        var exists = await _projectCommandRepository.TitleExistsAsync(
            title,
            null,
            cancellationToken);

        return exists
            ? Result.Failure(ProjectErrors.DuplicateTitle)
            : Result.Success();
    }

    private async Task<Result> EnsureAllSkillsExist(IReadOnlyList<Guid> skillIds, CancellationToken ct)
    {
        var count = await _skillCommandRepository.AllExistAsync(skillIds, ct);
        return count
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(SkillErrors.NotFound);
    }

    private static Result<Domain.Entities.Project> CreateProject(CreateProjectDTO request)
    {
        return Domain.Entities.Project.Create(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.LiveURL,
            request.GitHubURL);
    }

    private async Task<Result> AddProjectImages(Domain.Entities.Project project, IEnumerable<AddProjectImageDTO> images,
        List<(string Url, string PublicId)> uploadedImages,
        CancellationToken cancellationToken)
    {
        foreach (var image in images)
        {
            var uploadResult = await _imageStorageService.UploadImageAsync(
                image.Image,
                cancellationToken);

            if (uploadResult.IsFailure)
            {
                await CleanupUploadedImages(uploadedImages);

                return Result.Failure(uploadResult.Error!);
            }

            var uploadedImage = uploadResult.Value!;

            uploadedImages.Add((
                uploadedImage.URL,
                uploadedImage.PublicId));

            var addImageResult = project.AddProjectImage(
                uploadedImage.URL,
                uploadedImage.PublicId,
                image.AltText,
                image.DisplayOrder,
                image.IsCover);

            if (addImageResult.IsFailure)
            {
                await CleanupUploadedImages(uploadedImages);

                return Result.Failure(addImageResult.Error!);
            }
        }

        return Result.Success();
    }

    private async Task CleanupUploadedImages(IEnumerable<(string Url, string PublicId)> images)
    {
        foreach (var image in images)
        {
            await _imageStorageService.DeleteImageAsync(
                image.Url,
                image.PublicId);
        }
    }
}