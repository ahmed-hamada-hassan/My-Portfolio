using Application.Interfaces.DI;
using Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IDocumentStorageService : IScopedMark
{
    Task<Result<string>> UploadAsync(
        IFormFile file,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken);

    Task<Result<Stream>> OpenReadAsync(
        string fileUrl,
        CancellationToken cancellationToken);
}