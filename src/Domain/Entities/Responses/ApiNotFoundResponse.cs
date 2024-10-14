namespace Entities.Responses;

public abstract record ApiNotFoundResponse(string Message) : ApiBaseResponse(false);