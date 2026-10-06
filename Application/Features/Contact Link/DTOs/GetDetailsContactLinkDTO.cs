namespace Application.Features.Contact_Links.DTOs;

public sealed record GetDetailsContactLinkDTO(Guid Id, string Name, string Contact, int DisplayOrder, bool IsVisible);