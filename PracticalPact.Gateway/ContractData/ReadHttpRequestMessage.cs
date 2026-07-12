using System.Text.Json;

namespace PracticalPact.Gateway.ContractData;

public record ReadHttpRequestMessage(HttpRequestMessage Request, JsonElement? DeserializedBody)
{
	public static async Task<ReadHttpRequestMessage> Create(HttpRequestMessage request)
	{
		JsonElement? body = null;

		if (request.Content != null)
		{
			var content = await request.Content.ReadAsStringAsync();

			if (!string.IsNullOrWhiteSpace(content))
			{
				body = JsonSerializer.Deserialize<JsonElement>(content);
			}
		}

		return new ReadHttpRequestMessage(request, body);
	}

	public override string ToString()
	{
		return $"HttpRequestMessage with body:\n{DeserializedBody}";
	}
}