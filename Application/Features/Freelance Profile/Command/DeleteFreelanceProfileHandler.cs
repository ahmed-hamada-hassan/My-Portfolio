using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Freelance_Profile.Command;

internal sealed class DeleteFreelanceProfileHandler
    : ICommandHandler<DeleteFreelanceProfileCommand, Result>
{
    private readonly IFreelanceProfileCommandRepository _freelanceProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFreelanceProfileHandler> _logger;

    public DeleteFreelanceProfileHandler(
        IFreelanceProfileCommandRepository freelanceProfileRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteFreelanceProfileHandler> logger)
    {
        _freelanceProfileRepository = freelanceProfileRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        DeleteFreelanceProfileCommand command,
        CancellationToken cancellationToken)
    {
        var freelanceProfile =
            await _freelanceProfileRepository.GetByIdAsync(
                command.FreelanceProfileId,
                cancellationToken);

        if (freelanceProfile is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent FreelanceProfile. FreelanceProfileId: {FreelanceProfileId}",
                command.FreelanceProfileId);

            return Result.Failure(
                FreelanceProfileErrors.FreelanceProfileNotFound);
        }

        _freelanceProfileRepository.Remove(freelanceProfile);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "FreelanceProfile deleted successfully. FreelanceProfileId: {FreelanceProfileId}",
            command.FreelanceProfileId);

        return Result.Success();
    }
}