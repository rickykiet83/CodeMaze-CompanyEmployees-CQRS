using CompanyEmployees.Presentation.Commands;
using Contracts;
using Entities.Exceptions;

namespace CompanyEmployees.Presentation.Handlers;

public class DeleteCompanyHandler(IRepositoryManager repository) : IRequestHandler<DeleteCompanyCommand>
{
    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await repository.Company
            .GetCompanyAsync(request.Id, request.TrackChanges) ?? throw new CompanyNotFoundException(request.Id);
        
        repository.Company.DeleteCompany(company);
        await repository.SaveAsync();
    }
}