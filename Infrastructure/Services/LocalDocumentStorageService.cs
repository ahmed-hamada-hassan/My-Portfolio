using Application.Interfaces.Services;
using Domain.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

internal sealed class LocalDocumentStorageService : IDocumentStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalDocumentStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<Result<string>> UploadAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return Result<string>.Failure(
                new Domain.Shared.Error(
                    "DocumentStorage.EmptyFile",
                    "No file was provided."));

        var extension = Path.GetExtension(file.FileName);

        var fileName = $"{Guid.NewGuid()}{extension}";

        var directory = Path.Combine(
            _environment.WebRootPath,
            "Files",
            "CV");

        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.Create);

        await file.CopyToAsync(
            stream,
            cancellationToken);

        var url = $"/Files/CV/{fileName}";

        return Result<string>.Success(url);
    }

    public Task<Result> DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(fileUrl);

        var filePath = Path.Combine(
            _environment.WebRootPath,
            "Files",
            "CV",
            fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.FromResult(Result.Success());
    }

    public Task<Result<Stream>> OpenReadAsync(
    string fileUrl,
    CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileName = Path.GetFileName(fileUrl);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Task.FromResult(
                Result<Stream>.Failure(
                    new Error(
                        "DocumentStorage.InvalidFileUrl",
                        "The document URL is invalid.")));
        }

        var filePath = Path.Combine(
            _environment.WebRootPath,
            "Files",
            "CV",
            fileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult(
                Result<Stream>.Failure(
                    new Error(
                        "DocumentStorage.FileNotFound",
                        "The requested document was not found.")));
        }

        var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous |
                     FileOptions.SequentialScan);

        return Task.FromResult(
            Result<Stream>.Success(stream));
    }
}
