using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Contact_Links.Command;

internal sealed class UpdateContactLinkHandler : ICommandHandler<UpdateContactLinkCommand, Result>
{
    private readonly IContactLinkCommandRepository _socialLinkCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateContactLinkHandler> _logger;

    public UpdateContactLinkHandler(
        IContactLinkCommandRepository socialLinkCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateContactLinkHandler> logger)
    {
        _socialLinkCommandRepository = socialLinkCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(UpdateContactLinkCommand command, CancellationToken cancellationToken)
    {
        var request = command.updateContactLinkRequest;

        if (request.Name is not null &&
            await _socialLinkCommandRepository.NameExistsAsync(request.Name, command.contactLinkId, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update ContactLink with an existing name. ContactLinkId: {ContactLinkId}",
                command.contactLinkId);

            return Result.Failure(ContactLinkErrors.NameAlreadyExists);
        }

        if (request.Contact is not null &&
            await _socialLinkCommandRepository.URLExistsAsync(request.Contact, command.contactLinkId, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update ContactLink with an existing URL. ContactLinkId: {ContactLinkId}",
                command.contactLinkId);

            return Result.Failure(ContactLinkErrors.URLAlreadyExists);
        }

        if (request.DisplayOrder.HasValue &&
            await _socialLinkCommandRepository.DisplayOrderExistsAsync(request.DisplayOrder.Value, command.contactLinkId, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to update ContactLink with an existing display order. ContactLinkId: {ContactLinkId}",
                command.contactLinkId);

            return Result.Failure(ContactLinkErrors.DisplayOrderAlreadyExists);
        }

        var socialLink = await _socialLinkCommandRepository.GetByIdAsync(command.contactLinkId, cancellationToken);

        if (socialLink is null)
        {
            _logger.LogWarning(
                "Attempted to update a non-existent ContactLink. ContactLinkId: {ContactLinkId}",
                command.contactLinkId);

            return Result.Failure(ContactLinkErrors.SocialLinkNotFound);
        }

        var updateSocialLinkResult = socialLink.Update(request.Name, request.Contact, request.DisplayOrder, request.IsVisible);

        if (updateSocialLinkResult.IsFailure)
        {
            _logger.LogWarning(
                "ContactLink update failed due to domain validation. ContactLinkId: {ContactLinkId}, ErrorCode: {ErrorCode}",
                command.contactLinkId,
                updateSocialLinkResult.Error!.Code);

            return Result.Failure(updateSocialLinkResult.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ContactLink updated successfully. ContactLinkId: {ContactLinkId}",
            command.contactLinkId);

        return Result.Success();
    }
}
