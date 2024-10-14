namespace Entities.Responses;

public record CompanyNotFoundResponse(Guid Id) : ApiNotFoundResponse($"Company with id {Id} not found");
