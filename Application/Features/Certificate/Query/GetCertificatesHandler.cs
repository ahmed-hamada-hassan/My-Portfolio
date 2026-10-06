using Application.Features.Certificate.DTOs;
using Application.Interfaces.Repositories;
using Domain.Shared;
using Mediator;

namespace Application.Features.Certificate.Query;

internal sealed class GetCertificatesHandler : IQueryHandler<GetCertificatesQuery, Result<IReadOnlyList<GetCertificateDTO>>>
{
    private readonly ICertificateQueryRepository _certificateQueryRepository;

    public GetCertificatesHandler(ICertificateQueryRepository certificateQueryRepository)
    {
        _certificateQueryRepository = certificateQueryRepository;
    }

    public async ValueTask<Result<IReadOnlyList<GetCertificateDTO>>> Handle(GetCertificatesQuery query, CancellationToken cancellationToken)
    {
        var certificates = await _certificateQueryRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<GetCertificateDTO>>.Success(certificates);
    }
}
