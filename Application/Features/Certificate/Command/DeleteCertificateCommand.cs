using Domain.Shared;
using Mediator;

namespace Application.Features.Certificate.Command;

public sealed record DeleteCertificateCommand(Guid CertificateId) : ICommand<Result>;