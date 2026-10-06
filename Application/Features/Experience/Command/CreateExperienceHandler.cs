using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Experience.Command;

internal sealed class CreateExperienceHandler : ICommandHandler<CreateExperienceCommand, Result<Guid>>
{
    private readonly IExperienceCommandRepository _experienceCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateExperienceHandler> _logger;

    public CreateExperienceHandler(
        IExperienceCommandRepository experienceCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateExperienceHandler> logger)
    {
        _experienceCommandRepository = experienceCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(CreateExperienceCommand command, CancellationToken cancellationToken)
    {
        var request = command.createExperienceRequest;

        var experienceResult = Domain.Entities.Experience.Create(request.CompanyName, request.Position, request.Type, request.Description,
            request.StartDate, request.EndDate);

        if (experienceResult.IsFailure)
        {
            _logger.LogWarning(
                "Experience creation failed due to domain validation. ErrorCode: {ErrorCode}",
                experienceResult.Error!.Code);

            return Result<Guid>.Failure(experienceResult.Error!);
        }

        var experience = experienceResult.Value!;

        await _experienceCommandRepository.AddAsync(experience, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Experience created successfully. ExperienceId: {ExperienceId}",
            experience.Id);

        return Result<Guid>.Success(experience.Id);
    }
}
