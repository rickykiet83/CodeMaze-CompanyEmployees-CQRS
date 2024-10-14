namespace CompanyEmployees.Presentation.Commands;

public sealed record UpdateCompanyCommand
    (Guid Id, CompanyForUpdateDto Company, bool TrackChanges) : IRequest;