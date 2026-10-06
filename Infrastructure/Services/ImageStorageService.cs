using Application.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

internal sealed class ImageStorageService : IImageStorageService
{
    public readonly CloudinaryOptions _cloudinaryOptions;
    private readonly Cloudinary _cloudinary;

    public ImageStorageService(IOptionsSnapshot<CloudinaryOptions> cloudinaryOptions)
    {
        _cloudinaryOptions = cloudinaryOptions.Value;
        _cloudinary = new Cloudinary(new Account(_cloudinaryOptions.CloudName, _cloudinaryOptions.ApiKey, _cloudinaryOptions.ApiSecret));
    }

    public async Task<Result<(string URL, string PublicId)>> UploadImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
            return Result<(string URL, string PublicId)>.Failure(new Domain.Shared.Error("ImageStorageService.UploadImageAsync", "No image provided."));

        using var stream = image.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(image.FileName, stream)
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        var publicId = uploadResult.PublicId;
        var url = uploadResult.SecureUrl.ToString();

        if (uploadResult.Error is not null) 
            return Result<(string URL, string PublicId)>.Failure(new Domain.Shared.Error("ImageStorageService.UploadImageAsync", $"{uploadResult.Error.Message}"));

        return Result<(string URL, string PublicId)>.Success((url, publicId));
    }

    public async Task<Result<bool>> DeleteImageAsync(string imageUrl, string publicId)
    {
        if(string.IsNullOrWhiteSpace(imageUrl))
            return Result<bool>.Failure(new Domain.Shared.Error("ImageStorageService.DeleteImageAsync", "No image URL provided."));

        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        if(result.Error is not null)
            return Result<bool>.Failure(new Domain.Shared.Error("ImageStorageService.DeleteImageAsync", $"{result.Error.Message}"));

        return Result<bool>.Success(result.Result == "ok");
    }

}
