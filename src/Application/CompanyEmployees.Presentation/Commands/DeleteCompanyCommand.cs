namespace CompanyEmployees.Presentation.Commands;

public record DeleteCompanyCommand(Guid Id, bool TrackChanges) : IRequest;