using Application.Interfaces.DI;
using Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IImageStorageService : IScopedMark
{
    Task<Result<(string URL, string PublicId)>> UploadImageAsync(IFormFile image, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteImageAsync(string imageUrl, string publicId);
}