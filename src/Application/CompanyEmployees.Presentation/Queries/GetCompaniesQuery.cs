namespace CompanyEmployees.Presentation.Queries;

public sealed record GetCompaniesQuery(bool TrackChanges) : IRequest<IEnumerable<CompanyDto>>;