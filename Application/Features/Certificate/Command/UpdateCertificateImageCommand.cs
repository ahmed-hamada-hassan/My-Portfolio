using Domain.Shared;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Certificate.Command;

public sealed record UpdateCertificateImageCommand(Guid CertificateId, IFormFile CertificateImage) : ICommand<Result>;