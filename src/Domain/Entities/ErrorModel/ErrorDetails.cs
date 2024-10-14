using System.Text.Json;

namespace Entities.ErrorModel;

public record ErrorDetails(int StatusCode, string? Message)
{
	public override string ToString() => JsonSerializer.Serialize(this);
}

