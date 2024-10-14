using Asp.Versioning;
using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Presentation.Extensions;
using CompanyEmployees.Presentation.ModelBinders;
using Entities.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Presentation.Controllers;

[ApiVersion("1.0")]
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
public class CompaniesController(IServiceManager service) : ApiControllerBase
{
	/// <summary>
	/// Gets the list of all companies.
	/// </summary>
	/// <returns></returns>
	[HttpGet(Name = "GetCompanies")]
	// [ResponseCache(CacheProfileName = "120SecondsDuration")]
	[EnableRateLimiting("SpecificPolicy")]
	[Authorize(Roles = "Manager")]
	public async Task<IActionResult> GetCompanies()
	{
		var baseResult = await service.CompanyService.GetAllCompanies(false);
		var companies = baseResult.GetResult<IEnumerable<CompanyDto>>();
		return Ok(companies);
	}

	[HttpGet("{id:guid}", Name = "CompanyById")]
	// [ResponseCache(Duration = 60)]
	// [OutputCache(Duration = 60)]
	[DisableRateLimiting]
	public async Task<IActionResult> GetCompany(Guid id)
	{
		var baseResult = await service.CompanyService.GetCompany(id, false);
		if (!baseResult.Success)
			return ProcessError(baseResult);
		
		// ETag implementation (Need to use OutputCache attribute to see the 304-Not Modified response)
		var etag = $"\"{Guid.NewGuid():n}\"";
		HttpContext.Response.Headers.ETag = etag;
		
		var company = baseResult.GetResult<CompanyDto>();
		
		return Ok(company);
	}

	[HttpGet("collection/({ids})", Name = "CompanyCollection")]
	public async Task<IActionResult> GetCompanyCollection
		([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
	{
		var companies = await service.CompanyService.GetByIdsAsync(ids, trackChanges: false);

		return Ok(companies);
	}

	[HttpPost(Name = "CreateCompany")]
	[ServiceFilter(typeof(ValidationFilterAttribute))]
	public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto? company)
	{
		var createdCompany = await service.CompanyService.CreateCompanyAsync(company);

		return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
	}

	[HttpPost("collection")]
	public async Task<IActionResult> CreateCompanyCollection
		([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
	{
		var result = await service.CompanyService.CreateCompanyCollectionAsync(companyCollection);

		return CreatedAtRoute("CompanyCollection", new { result.ids }, result.companies);
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> DeleteCompany(Guid id)
	{
		await service.CompanyService.DeleteCompanyAsync(id, trackChanges: false);

		return NoContent();
	}

	[HttpPut("{id:guid}")]
	[ServiceFilter(typeof(ValidationFilterAttribute))]
	public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company)
	{
		await service.CompanyService.UpdateCompanyAsync(id, company, trackChanges: true);

		return NoContent();
	}
	
	[HttpOptions]
	public IActionResult GetCompaniesOptions()
	{
		Response.Headers.Append("Allow", "GET, POST, PUT, DELETE, OPTIONS");
		return Ok();
	}
}
