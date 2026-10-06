using Domain.Shared;
using Mediator;

namespace Application.Features.Certificate.Command;

public sealed record ClearCredentialURLCommand(Guid CertificateId) : ICommand<Result>;
