using AutoMapper;
using CompanyEmployees.Presentation.Commands;
using Contracts;
using Entities.Models;

namespace CompanyEmployees.Presentation.Handlers;

public class CreateCompanyHandler(IRepositoryManager repository, IMapper mapper) : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyEntity = mapper.Map<Company>(request.Company);
        repository.Company.CreateCompany(companyEntity);
        await repository.SaveAsync();

        var companyToReturn = mapper.Map<CompanyDto>(companyEntity);
        return companyToReturn;
    }
}