namespace Application.Shared;

public record PaginationRequest(int Page = 1, byte PageSize = 10);