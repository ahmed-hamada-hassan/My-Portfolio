namespace Application.Features.Freelance_Profile.DTOs;

public sealed record CreateFreelanceProfileDTO(string PlatformName, string ProfileURL, string Icon, byte DisplayOrder, bool IsVisible);