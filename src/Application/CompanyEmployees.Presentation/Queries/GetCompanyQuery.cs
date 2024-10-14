namespace CompanyEmployees.Presentation.Queries;

public sealed record GetCompanyQuery(Guid Id, bool TrackChanges) : IRequest<CompanyDto>;