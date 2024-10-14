using Asp.Versioning;
using CompanyEmployees.Presentation.Commands;
using CompanyEmployees.Presentation.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Presentation.Controllers;

[ApiVersion("1.0")]
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
public class CompaniesController(ISender sender) : ApiControllerBase
{
	/// <summary>
	/// Gets the list of all companies.
	/// </summary>
	/// <returns></returns>
	[HttpGet(Name = "GetCompanies")]
	public async Task<IActionResult> GetCompanies()
	{
		var companies = await sender.Send(new GetCompaniesQuery(TrackChanges: false));
		return Ok(companies);
	}
	
	[HttpGet("{id:guid}", Name = "CompanyById")]
	public async Task<IActionResult> GetCompany(Guid id)
	{
		var company = await sender.Send(new GetCompanyQuery(id, TrackChanges: false));
		return Ok(company);
	}
	
	[HttpPost]
	public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto companyForCreationDto)
	{
		if (companyForCreationDto is null)
			return BadRequest("CompanyForCreationDto object is null");
		
		var company = await sender.Send(new CreateCompanyCommand(companyForCreationDto));
		return CreatedAtRoute("CompanyById", new { id = company.Id }, company);
	}
	
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> UpdateCompany(Guid id, CompanyForUpdateDto
		companyForUpdateDto)
	{
		if (companyForUpdateDto is null)
			return BadRequest("CompanyForUpdateDto object is null");
		
		await sender.Send(new UpdateCompanyCommand(id, companyForUpdateDto, 
			TrackChanges: true));
		return NoContent();
	}
	
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> DeleteCompany(Guid id)
	{
		await sender.Send(new DeleteCompanyCommand(id, TrackChanges: false));
		return NoContent();
	}

	[HttpOptions]
	public IActionResult GetCompaniesOptions()
	{
		Response.Headers.Append("Allow", "GET, POST, PUT, DELETE, OPTIONS");
		return Ok();
	}
}
