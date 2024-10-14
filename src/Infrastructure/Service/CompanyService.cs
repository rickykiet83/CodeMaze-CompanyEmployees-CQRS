using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Entities.Responses;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace Service;

internal sealed class CompanyService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
	: ICompanyService
{
	public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync(bool trackChanges)
	{
		var companies = await repository.Company.GetAllCompaniesAsync(trackChanges);

		var companiesDto = mapper.Map<IEnumerable<CompanyDto>>(companies);

		return companiesDto;
	}

	public async Task<CompanyDto> GetCompanyAsync(Guid id, bool trackChanges)
	{
		var company = await GetCompanyAndCheckIfItExists(id, trackChanges);

		var companyDto = mapper.Map<CompanyDto>(company);
		return companyDto;
	}

	public async Task<CompanyDto> CreateCompanyAsync(CompanyForCreationDto company)
	{
		var companyEntity = mapper.Map<Company>(company);

		repository.Company.CreateCompany(companyEntity);
		await repository.SaveAsync();

		var companyToReturn = mapper.Map<CompanyDto>(companyEntity);

		return companyToReturn;
	}

	public async Task<IEnumerable<CompanyDto>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
	{
		if (ids is null)
			throw new IdParametersBadRequestException();

		var companyEntities = await repository.Company.GetByIdsAsync(ids, trackChanges);
		if (ids.Count() != companyEntities.Count())
			throw new CollectionByIdsBadRequestException();

		var companiesToReturn = mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

		return companiesToReturn;
	}

	public async Task<(IEnumerable<CompanyDto> companies, string ids)> CreateCompanyCollectionAsync
		(IEnumerable<CompanyForCreationDto> companyCollection)
	{
		if (companyCollection is null)
			throw new CompanyCollectionBadRequest();

		var companyEntities = mapper.Map<IEnumerable<Company>>(companyCollection);
		foreach (var company in companyEntities)
			repository.Company.CreateCompany(company);

		await repository.SaveAsync();

		var companyCollectionToReturn = mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
		var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

		return (companies: companyCollectionToReturn, ids: ids);
	}

	public async Task DeleteCompanyAsync(Guid companyId, bool trackChanges)
	{
		var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);

		repository.Company.DeleteCompany(company);
		await repository.SaveAsync();
	}

	public async Task UpdateCompanyAsync(Guid companyId,
		CompanyForUpdateDto companyForUpdate, bool trackChanges)
	{
		var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);

		mapper.Map(companyForUpdate, company);
		await repository.SaveAsync();
	}

	public async Task<ApiBaseResponse> GetAllCompanies(bool trackChanges)
	{
		var companies = await repository.Company.GetAllCompaniesAsync(trackChanges);
		var companiesDto = mapper.Map<IEnumerable<CompanyDto>>(companies);
		return new ApiOkResponse<IEnumerable<CompanyDto>>(companiesDto);
	}

	public async Task<ApiBaseResponse> GetCompany(Guid companyId, bool trackChanges)
	{
		var company = await repository.Company.GetCompanyAsync(companyId, trackChanges);
		if (company is null)
			return new CompanyNotFoundResponse(companyId);
		
		var companyDto = mapper.Map<CompanyDto>(company);
		return new ApiOkResponse<CompanyDto>(companyDto);
	}

	private async Task<Company> GetCompanyAndCheckIfItExists(Guid id, bool trackChanges)
	{
		var company = await repository.Company.GetCompanyAsync(id, trackChanges);
		if (company is null)
			throw new CompanyNotFoundException(id);

		return company;
	}
}
