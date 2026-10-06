using Application.Features.Certificate.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Certificate.Command;

public sealed record CreateCertificateCommand(CreateCertificateDTO createCertificateRequest) : ICommand<Result<Guid>>;
