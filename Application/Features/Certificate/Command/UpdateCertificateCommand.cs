using Application.Features.Certificate.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Certificate.Command;

public sealed record UpdateCertificateCommand(Guid CertificateId, UpdateCertificateDTO UpdateCertificateRequest) : ICommand<Result>;