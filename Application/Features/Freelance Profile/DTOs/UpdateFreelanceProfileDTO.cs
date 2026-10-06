namespace Application.Features.Freelance_Profile.DTOs;

public sealed record UpdateFreelanceProfileDTO(string? PlatformName, string? ProfileURL, string? Icon, byte? DisplayOrder, bool? IsVisible);