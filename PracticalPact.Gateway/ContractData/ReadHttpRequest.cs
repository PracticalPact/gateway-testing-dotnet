using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PracticalPact.Gateway.ContractData;

public record ReadHttpRequest(HttpRequest Request, JsonElement? DeserializedBody)
{
	public static async Task<ReadHttpRequest> Create(HttpRequest request)
	{
		return new ReadHttpRequest(request, await ReadBody(request));
	}

	private static async Task<JsonElement?> ReadBody(HttpRequest request)
	{
		if (request.Body == null)
		{
			return null;
		}

		request.EnableBuffering();
		request.Body.Position = 0;

		using var reader = new StreamReader(
			request.Body,
			leaveOpen: true);

		var content = await reader.ReadToEndAsync();

		request.Body.Position = 0;

		if (string.IsNullOrWhiteSpace(content))
		{
			return null;
		}

		return JsonSerializer.Deserialize<JsonElement>(content);
	}

	public override string ToString()
	{
		return $"HttpRequest with body:\n{DeserializedBody}";
	}
}