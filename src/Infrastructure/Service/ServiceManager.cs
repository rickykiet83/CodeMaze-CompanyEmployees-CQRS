using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace Service;

public sealed class ServiceManager(
	IRepositoryManager repositoryManager,
	ILoggerManager logger,
	IMapper mapper,
	IDataShaper<EmployeeDto> dataShaper,
	IEmployeeLinks employeeLinks,
	UserManager<User> userManager,
	IOptions<JwtConfiguration> configuration
	)
	: IServiceManager
{
	private readonly Lazy<ICompanyService> _companyService = new(() => new CompanyService(repositoryManager, logger, mapper));
	private readonly Lazy<IEmployeeService> _employeeService = new(() => new EmployeeService(repositoryManager, logger, mapper, dataShaper, employeeLinks));
	private readonly Lazy<IAuthenticationService> _authenticationService = new(() => new AuthenticationService(logger, mapper, userManager, configuration));

	public ICompanyService CompanyService => _companyService.Value;
	public IEmployeeService EmployeeService => _employeeService.Value;
	public IAuthenticationService AuthenticationService => _authenticationService.Value;
}
