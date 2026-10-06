using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Contact_Links.Command;

internal sealed class DeleteContactLinkHandler : ICommandHandler<DeleteContactLinkCommand, Result>
{
    private readonly IContactLinkCommandRepository _contactLinkCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteContactLinkHandler> _logger;

    public DeleteContactLinkHandler(
        IContactLinkCommandRepository contactLinkCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteContactLinkHandler> logger)
    {
        _contactLinkCommandRepository = contactLinkCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(DeleteContactLinkCommand command, CancellationToken cancellationToken)
    {
        var socialLink = await _contactLinkCommandRepository.GetByIdAsync(command.contactLinkId, cancellationToken);

        if (socialLink is null)
        {
            _logger.LogWarning(
                "Attempted to delete a non-existent ContactLink. ContactLinkId: {ContactLinkId}",
                command.contactLinkId);

            return Result.Failure(ContactLinkErrors.SocialLinkNotFound);
        }

        _contactLinkCommandRepository.Remove(socialLink);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ContactLink deleted successfully. ContactLinkId: {ContactLinkId}",
            command.contactLinkId);

        return Result.Success();
    }
}
