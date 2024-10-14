namespace Entities.Responses;

public sealed record ApiOkResponse<TResult>(TResult Result) : ApiBaseResponse(true);