using Microsoft.AspNetCore.Http;

namespace Application.Features.Project_Images.DTOs;

public record UpdateProjectImageDTO(IFormFile? Image, string? AltText, byte? DisplayOrder, bool? IsCover);