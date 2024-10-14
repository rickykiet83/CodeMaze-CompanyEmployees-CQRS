using AutoMapper;
using CompanyEmployees.Presentation.Commands;
using Contracts;
using Entities.Exceptions;

namespace CompanyEmployees.Presentation.Handlers;

internal sealed class UpdateCompanyHandler(IRepositoryManager repository, IMapper mapper) : IRequestHandler<UpdateCompanyCommand>
{
    public async Task Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyEntity = await repository.Company
            .GetCompanyAsync(request.Id, request.TrackChanges);
        if (companyEntity is null)
            throw new CompanyNotFoundException(request.Id);
        
        mapper.Map(request.Company, companyEntity);
        await repository.SaveAsync();
    }
}