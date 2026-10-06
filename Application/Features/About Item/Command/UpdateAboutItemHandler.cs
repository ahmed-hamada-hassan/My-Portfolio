using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.About_Item.Command;

internal sealed class UpdateAboutItemHandler
    : ICommandHandler<UpdateAboutItemCommand, Result>
{
    private readonly IAboutItemCommandRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAboutItemHandler> _logger;

    public UpdateAboutItemHandler(
        IAboutItemCommandRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAboutItemHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        UpdateAboutItemCommand command,
        CancellationToken cancellationToken)
    {
        var aboutItem = await _repository.GetByIdAsync(
            command.AboutItemId,
            cancellationToken);

        if (aboutItem is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent AboutItem. AboutItemId: {AboutItemId}",
                command.AboutItemId);

            return Result.Failure(
                AboutItemErrors.AboutItemNotFound);
        }

        var request = command.UpdateAboutItemRequest;

        if (request.Title is not null &&
            await _repository.TitleExistsAsync(
                request.Title,
                command.AboutItemId,
                cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update AboutItem with an existing title. AboutItemId: {AboutItemId}",
                command.AboutItemId);

            return Result.Failure(
                AboutItemErrors.TitleAlreadyExists);
        }

        if (request.DisplayOrder.HasValue &&
            await _repository.DisplayOrderExistsAsync(
                request.DisplayOrder.Value,
                command.AboutItemId,
                cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update AboutItem with an existing display order. AboutItemId: {AboutItemId}",
                command.AboutItemId);

            return Result.Failure(
                AboutItemErrors.DisplayOrderAlreadyExists);
        }

        var result = aboutItem.Update(
            request.Title,
            request.Description,
            request.DisplayOrder,
            request.IsVisible);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "AboutItem update failed due to domain validation. AboutItemId: {AboutItemId}, ErrorCode: {ErrorCode}",
                command.AboutItemId,
                result.Error!.Code);

            return Result.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "AboutItem updated successfully. AboutItemId: {AboutItemId}",
            command.AboutItemId);

        return Result.Success();
    }
}