using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.RequestFeatures;
using Repository.Extensions;

namespace Repository;

internal sealed class EmployeeRepository(RepositoryContext repositoryContext)
    : RepositoryBase<Employee>(repositoryContext), IEmployeeRepository
{
    public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters,
        bool trackChanges)
    {
        var employees =  FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
            .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
            .Search(employeeParameters.SearchTerm)
            .Sort(employeeParameters.OrderBy);
        
        return await PagedList<Employee>
            .ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize)
            .ConfigureAwait(false);
    }

    public async Task<Employee> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges) =>
        await FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges)
            .SingleOrDefaultAsync();

    public void CreateEmployeeForCompany(Guid companyId, Employee employee)
    {
        employee.CompanyId = companyId;
        Create(employee);
    }

    public void DeleteEmployee(Employee employee) => Delete(employee);
}