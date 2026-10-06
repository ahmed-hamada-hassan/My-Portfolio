namespace Application.Features.About_Item.DTOs;

public sealed record GetDetailsAboutItemDTO(
    Guid Id,
    string Title,
    string Description,
    byte DisplayOrder,
    bool IsVisible);