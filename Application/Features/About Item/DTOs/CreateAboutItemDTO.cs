namespace Application.Features.About_Item.DTOs;

public sealed record CreateAboutItemDTO(
    string Title,
    string Description,
    byte DisplayOrder,
    bool IsVisible);