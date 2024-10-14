using Entities.ErrorModel;
using Entities.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Presentation.Controllers;

public class ApiControllerBase : ControllerBase
{
    protected IActionResult ProcessError(ApiBaseResponse baseResponse)
    {
        return baseResponse switch
        {
            ApiNotFoundResponse response => NotFound(new ErrorDetails(StatusCodes.Status404NotFound, response.Message)),
            ApiBadRequestResponse response => BadRequest(new ErrorDetails(StatusCodes.Status400BadRequest, response.Message)),
            _ => throw new NotImplementedException()
        };
    }
}