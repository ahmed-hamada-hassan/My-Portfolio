using Application.Features.Contact_Links.DTOs;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Query;

internal sealed class GetDetailedContactLinkHandler : IQueryHandler<GetDetailedContactLinkQuery, Result<GetDetailsContactLinkDTO>>
{
    private readonly IContactLinkQueryRepository _socialLinkQueryRepository;
    public GetDetailedContactLinkHandler(IContactLinkQueryRepository socialLinkQueryRepository)
    {
        _socialLinkQueryRepository = socialLinkQueryRepository;
    }
    public async ValueTask<Result<GetDetailsContactLinkDTO>> Handle(GetDetailedContactLinkQuery query, CancellationToken cancellationToken)
    {
        var contactLink = await _socialLinkQueryRepository.GetDetailsByIdAsync(query.ContactLinkId, cancellationToken);
        if (contactLink is null)
            return Result<GetDetailsContactLinkDTO>.Failure(ContactLinkErrors.SocialLinkNotFound);

        return Result<GetDetailsContactLinkDTO>.Success(contactLink);
    }
}
