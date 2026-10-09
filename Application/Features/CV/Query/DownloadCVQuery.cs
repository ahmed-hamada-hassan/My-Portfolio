using Domain.Shared;
using Mediator;

namespace Application.Features.CV.Query;

public sealed record DownloadCVQuery : IQuery<Result<Stream>>;