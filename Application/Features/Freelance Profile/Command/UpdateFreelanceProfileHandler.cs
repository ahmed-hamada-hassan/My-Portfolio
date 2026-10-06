using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Freelance_Profile.Command;

internal sealed class UpdateFreelanceProfileHandler
    : ICommandHandler<UpdateFreelanceProfileCommand, Result>
{
    private readonly IFreelanceProfileCommandRepository _freelanceProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFreelanceProfileHandler> _logger;

    public UpdateFreelanceProfileHandler(
        IFreelanceProfileCommandRepository freelanceProfileRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFreelanceProfileHandler> logger)
    {
        _freelanceProfileRepository = freelanceProfileRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        UpdateFreelanceProfileCommand command,
        CancellationToken cancellationToken)
    {
        var freelanceProfile =
            await _freelanceProfileRepository.GetByIdAsync(
                command.FreelanceProfileId,
                cancellationToken);

        if (freelanceProfile is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent FreelanceProfile. FreelanceProfileId: {FreelanceProfileId}",
                command.FreelanceProfileId);

            return Result.Failure(
                FreelanceProfileErrors.FreelanceProfileNotFound);
        }

        var request = command.UpdateFreelanceProfileRequest;

        if (request.PlatformName is not null &&
            await _freelanceProfileRepository.PlatformNameExistsAsync(
                request.PlatformName,
                command.FreelanceProfileId,
                cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update FreelanceProfile with an existing platform name. FreelanceProfileId: {FreelanceProfileId}",
                command.FreelanceProfileId);

            return Result.Failure(
                FreelanceProfileErrors.PlatformNameAlreadyExists);
        }

        if (request.ProfileURL is not null &&
            await _freelanceProfileRepository.ProfileURLExistsAsync(
                request.ProfileURL,
                command.FreelanceProfileId,
                cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update FreelanceProfile with an existing profile URL. FreelanceProfileId: {FreelanceProfileId}",
                command.FreelanceProfileId);

            return Result.Failure(
                FreelanceProfileErrors.ProfileUrlAlreadyExists);
        }

        if (request.Icon is not null &&
            await _freelanceProfileRepository.IconExistsAsync(
                request.Icon,
                command.FreelanceProfileId,
                cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update FreelanceProfile with an existing icon. FreelanceProfileId: {FreelanceProfileId}",
                command.FreelanceProfileId);

            return Result.Failure(
                FreelanceProfileErrors.IconAlreadyExists);
        }

        if (request.DisplayOrder.HasValue &&
            await _freelanceProfileRepository.DisplayOrderExistsAsync(
                request.DisplayOrder.Value,
                command.FreelanceProfileId,
                cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update FreelanceProfile with an existing display order. FreelanceProfileId: {FreelanceProfileId}",
                command.FreelanceProfileId);

            return Result.Failure(
                FreelanceProfileErrors.DisplayOrderAlreadyExists);
        }

        var result = freelanceProfile.Update(
            request.PlatformName,
            request.ProfileURL,
            request.Icon,
            request.DisplayOrder,
            request.IsVisible);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "FreelanceProfile update failed due to domain validation. FreelanceProfileId: {FreelanceProfileId}, ErrorCode: {ErrorCode}",
                command.FreelanceProfileId,
                result.Error!.Code);

            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "FreelanceProfile updated successfully. FreelanceProfileId: {FreelanceProfileId}",
            command.FreelanceProfileId);

        return Result.Success();
    }
}