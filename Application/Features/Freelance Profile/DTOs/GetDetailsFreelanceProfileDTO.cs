namespace Application.Features.Freelance_Profile.DTOs;

public sealed record GetDetailsFreelanceProfileDTO(Guid Id, string PlatformName, string ProfileURL, string Icon, byte DisplayOrder, bool IsVisible);