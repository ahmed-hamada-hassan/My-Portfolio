using Application.Features.Contact_Links.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.Contact_Links.Query;

internal sealed class GetContactLinksHandler : IQueryHandler<GetContactLinksQuery, Result<IReadOnlyList<GetContactLinkDTO>>>
{
    private readonly IContactLinkQueryRepository _socialLinkQueryRepository;

    public GetContactLinksHandler(IContactLinkQueryRepository socialLinkQueryRepository)
    {
        _socialLinkQueryRepository = socialLinkQueryRepository;
    }

    public async ValueTask<Result<IReadOnlyList<GetContactLinkDTO>>> Handle(GetContactLinksQuery query, CancellationToken cancellationToken)
    {
        var socialLinks = await _socialLinkQueryRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<GetContactLinkDTO>>.Success(socialLinks);
    }
}
