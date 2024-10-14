namespace CompanyEmployees.Presentation.Commands;

public record CreateCompanyCommand(CompanyForCreationDto Company) : IRequest<CompanyDto>;