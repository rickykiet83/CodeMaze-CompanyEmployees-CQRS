using AutoMapper;
using CompanyEmployees.Presentation.Queries;
using Contracts;
using Entities.Exceptions;

namespace CompanyEmployees.Presentation.Handlers;

internal sealed class GetCompanyHandler(IRepositoryManager repository, IMapper mapper) : IRequestHandler<GetCompanyQuery, CompanyDto>
{
    public async Task<CompanyDto> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await repository.Company.GetCompanyAsync(request.Id, request.TrackChanges) ?? throw new CompanyNotFoundException(request.Id);
        var companyDto = mapper.Map<CompanyDto>(company);

        return companyDto;
    }
}