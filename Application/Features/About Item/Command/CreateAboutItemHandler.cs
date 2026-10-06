using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.About_Item.Command;

internal sealed class CreateAboutItemHandler
    : ICommandHandler<CreateAboutItemCommand, Result<Guid>>
{
    private readonly IAboutItemCommandRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAboutItemHandler> _logger;

    public CreateAboutItemHandler(IAboutItemCommandRepository repository, IUnitOfWork unitOfWork, ILogger<CreateAboutItemHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(
        CreateAboutItemCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.CreateAboutItemRequest;

        if (await _repository.TitleExistsAsync(request.Title, cancellationToken: cancellationToken))
        {
            _logger.LogWarning("Attempt to create AboutItem with existing title");
            return Result<Guid>.Failure(AboutItemErrors.TitleAlreadyExists);
        }

        if (await _repository.DisplayOrderExistsAsync(request.DisplayOrder, cancellationToken: cancellationToken))
        {
            _logger.LogWarning("Attempt to create AboutItem with existing display order");
            return Result<Guid>.Failure(AboutItemErrors.DisplayOrderAlreadyExists);
        }

        var result = AboutItem.Create(request.Title, request.Description, request.DisplayOrder, request.IsVisible);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "AboutItem creation failed due to domain validation. ErrorCode: {ErrorCode}",
                result.Error!.Code);
            return Result<Guid>.Failure(result.Error!);
        }

        var aboutItem = result.Value!;

        await _repository.AddAsync(aboutItem, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "AboutItem created successfully. AboutItemId: {AboutItemId}",
            aboutItem.Id);

        return Result<Guid>.Success(aboutItem.Id);
    }
}