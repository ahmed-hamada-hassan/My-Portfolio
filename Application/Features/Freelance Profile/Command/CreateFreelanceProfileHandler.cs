using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Freelance_Profile.Command;

internal sealed class CreateFreelanceProfileHandler : ICommandHandler<CreateFreelanceProfileCommand, Result<Guid>>
{
    private readonly IFreelanceProfileCommandRepository _freelanceProfileCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFreelanceProfileHandler> _logger;

    public CreateFreelanceProfileHandler(
        IFreelanceProfileCommandRepository freelanceProfileCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFreelanceProfileHandler> logger)
    {
        _freelanceProfileCommandRepository = freelanceProfileCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(CreateFreelanceProfileCommand command, CancellationToken cancellationToken)
    {
        var request = command.createFreelanceProfileRequest;

        if (await _freelanceProfileCommandRepository.PlatformNameExistsAsync(request.PlatformName, null, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to create FreelanceProfile with an existing platform name.");

            return Result<Guid>.Failure(FreelanceProfileErrors.PlatformNameAlreadyExists);
        }

        if (await _freelanceProfileCommandRepository.ProfileURLExistsAsync(request.ProfileURL, null, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to create FreelanceProfile with an existing profile URL.");

            return Result<Guid>.Failure(FreelanceProfileErrors.ProfileUrlAlreadyExists);
        }

        if (await _freelanceProfileCommandRepository.IconExistsAsync(request.Icon, null, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to create FreelanceProfile with an existing icon.");

            return Result<Guid>.Failure(FreelanceProfileErrors.IconAlreadyExists);
        }

        if (await _freelanceProfileCommandRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to create FreelanceProfile with an existing display order.");

            return Result<Guid>.Failure(FreelanceProfileErrors.DisplayOrderAlreadyExists);
        }

        var createProfileResult = FreelanceProfile.Create(request.PlatformName, request.ProfileURL, request.Icon,
            request.DisplayOrder, request.IsVisible);

        if (createProfileResult.IsFailure)
        {
            _logger.LogWarning(
                "FreelanceProfile creation failed due to domain validation. ErrorCode: {ErrorCode}",
                createProfileResult.Error!.Code);

            return Result<Guid>.Failure(createProfileResult.Error!);
        }

        var freelanceProfile = createProfileResult.Value!;

        await _freelanceProfileCommandRepository.AddAsync(freelanceProfile, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "FreelanceProfile created successfully. FreelanceProfileId: {FreelanceProfileId}",
            freelanceProfile.Id);

        return Result<Guid>.Success(freelanceProfile.Id);
    }
}