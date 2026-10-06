using Application.Features.Certificate.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Certificate.Query;

public sealed record GetCertificatesQuery() : IQuery<Result<IReadOnlyList<GetCertificateDTO>>>;