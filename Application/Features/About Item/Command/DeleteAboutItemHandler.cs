using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.About_Item.Command;

internal sealed class DeleteAboutItemHandler
    : ICommandHandler<DeleteAboutItemCommand, Result>
{
    private readonly IAboutItemCommandRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAboutItemHandler> _logger;

    public DeleteAboutItemHandler(
        IAboutItemCommandRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAboutItemHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        DeleteAboutItemCommand command,
        CancellationToken cancellationToken)
    {
        var aboutItem = await _repository.GetByIdAsync(command.AboutItemId, cancellationToken);

        if (aboutItem is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent AboutItem. AboutItemId: {AboutItemId}",
                command.AboutItemId);
            return Result.Failure(AboutItemErrors.AboutItemNotFound);
        }

        _repository.Remove(aboutItem);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "AboutItem deleted successfully. AboutItemId: {AboutItemId}",
            command.AboutItemId);

        return Result.Success();
    }
}