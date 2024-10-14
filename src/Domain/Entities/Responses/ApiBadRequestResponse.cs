namespace Entities.Responses;

public abstract record ApiBadRequestResponse(string Message) : ApiBaseResponse(false);
