using Domain.Enums;

namespace Application.Features.Contact_Links.DTOs;

public sealed record UpdateContactLinkDTO(
    string? Name,
    string? Contact,
    byte? DisplayOrder,
    bool? IsVisible);