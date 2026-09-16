using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Command;

internal sealed class CreateProjectHandler : ICommandHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IProjectCommandRepository _projectCommandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectHandler(IProjectCommandRepository projectCommandRepository, IUnitOfWork unitOfWork)
    {
        _projectCommandRepository = projectCommandRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<Guid>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var request = command.CreateProjectRequest;

        if (await _projectCommandRepository.TitleExistsAsync(
                request.Title,
                cancellationToken))
        {
            return Result<Guid>.Failure(ProjectErrors.DuplicateTitle);
        }

        var result = Domain.Entities.Project.Create(request.Title, request.Description,
            request.StartDate, request.EndDate,
            request.LiveURL, request.GitHubURL);

        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error!);

        var project = result.Value!;

        await _projectCommandRepository.AddAsync(project, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(project.Id);
    }
}
