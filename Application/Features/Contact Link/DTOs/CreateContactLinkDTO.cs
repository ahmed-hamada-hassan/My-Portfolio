namespace Application.Features.Contact_Links.DTOs;

public record CreateContactLinkDTO(
    string Name,
    string Contact,
    byte DisplayOrder,
    bool IsVisible);
