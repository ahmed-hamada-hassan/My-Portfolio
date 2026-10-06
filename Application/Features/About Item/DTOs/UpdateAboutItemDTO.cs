namespace Application.Features.About_Item.DTOs;

public sealed record UpdateAboutItemDTO(
    string? Title,
    string? Description,
    byte? DisplayOrder,
    bool? IsVisible);