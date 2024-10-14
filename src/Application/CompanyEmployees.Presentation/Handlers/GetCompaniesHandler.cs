using AutoMapper;
using CompanyEmployees.Presentation.Queries;
using Contracts;

namespace CompanyEmployees.Presentation.Handlers;

public sealed class GetCompaniesHandler(IRepositoryManager repository, IMapper mapper) : IRequestHandler<GetCompaniesQuery, IEnumerable<CompanyDto>>
{
    public async Task<IEnumerable<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await repository.Company.GetAllCompaniesAsync(request.TrackChanges);
        var companiesDto = mapper.Map<IEnumerable<CompanyDto>>(companies);

        return companiesDto;
    }
}