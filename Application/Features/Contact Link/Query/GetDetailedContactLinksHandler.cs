using Application.Features.Contact_Links.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Query;

internal sealed class GetDetailedContactLinksHandler : IQueryHandler<GetDetailedContactLinksQuery, Result<IReadOnlyList<GetDetailsContactLinkDTO>>>
{
    private readonly IContactLinkQueryRepository _socialLinkQueryRepository;
    public GetDetailedContactLinksHandler(IContactLinkQueryRepository socialLinkQueryRepository)
    {
        _socialLinkQueryRepository = socialLinkQueryRepository;
    }
    public async ValueTask<Result<IReadOnlyList<GetDetailsContactLinkDTO>>> Handle(GetDetailedContactLinksQuery query, CancellationToken cancellationToken)
    {
        var contactLinks = await _socialLinkQueryRepository.GetDetailsAsync(cancellationToken);
        return Result<IReadOnlyList<GetDetailsContactLinkDTO>>.Success(contactLinks);
    }
}
