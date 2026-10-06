using Application.Interfaces.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Contact_Links.Command;

internal sealed class CreateContactLinkHandler : ICommandHandler<CreateContactLinkCommand, Result<Guid>>
{
    private readonly IContactLinkCommandRepository _contactLinkCommandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateContactLinkHandler> _logger;

    public CreateContactLinkHandler(
        IContactLinkCommandRepository contactLinkCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateContactLinkHandler> logger)
    {
        _contactLinkCommandRepository = contactLinkCommandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(CreateContactLinkCommand command, CancellationToken cancellationToken)
    {
        var request = command.CreateContactLinkRequest;

        if (await _contactLinkCommandRepository.NameExistsAsync(request.Name, null, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to create ContactLink with an existing name.");

            return Result<Guid>.Failure(ContactLinkErrors.NameAlreadyExists);
        }

        if (await _contactLinkCommandRepository.URLExistsAsync(request.Contact, null, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to create ContactLink with an existing Contact.");

            return Result<Guid>.Failure(ContactLinkErrors.URLAlreadyExists);
        }

        if (await _contactLinkCommandRepository.DisplayOrderExistsAsync(request.DisplayOrder, null, cancellationToken))
        {
            _logger.LogWarning(
                "Attempted to create ContactLink with an existing display order.");

            return Result<Guid>.Failure(ContactLinkErrors.DisplayOrderAlreadyExists);
        }

        var socialLinkResult = ContactLink.Create(request.Name, request.Contact, request.DisplayOrder, request.IsVisible);
        if (socialLinkResult.IsFailure)
        {
            _logger.LogWarning(
                "ContactLink creation failed due to domain validation. ErrorCode: {ErrorCode}",
                socialLinkResult.Error!.Code);

            return Result<Guid>.Failure(socialLinkResult.Error!);
        }

        var socialLink = socialLinkResult.Value!;

        await _contactLinkCommandRepository.AddAsync(socialLink, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ContactLink created successfully. ContactLinkId: {ContactLinkId}",
            socialLink.Id);

        return Result<Guid>.Success(socialLink.Id);
    }
}
