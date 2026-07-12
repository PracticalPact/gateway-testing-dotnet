using System.Net;
using System.Text;
using System.Text.Json;

namespace PracticalPact.Gateway.ContractData;

public class Response
{
	public JsonElement? Body { get; set; }
	public JsonElement? Headers { get; set; }
	public JsonElement? MatchingRules { get; set; }
	public required int Status { get; set; }

	public HttpResponseMessage CreateResponse()
	{
		var response = new HttpResponseMessage
		{
			StatusCode = (HttpStatusCode)Status
		};

		// Preserve original JSON body
		var bodyJson = Body != null ? Body!.Value.GetProperty("content").GetRawText() : "";

		response.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

		if (Headers != null)
		{
			response.Content ??= new StringContent("");

			foreach (var header in Headers.Value.EnumerateObject())
			{
				var values = header.Value
					.EnumerateArray()
					.Select(x => x.GetString())
					.Where(x => !string.IsNullOrEmpty(x))
					.Cast<string>()
					.ToArray();

				if (values.Length == 0)
				{
					continue;
				}

				// Content-Type requires special handling
				if (header.Name.Equals(
					    "Content-Type",
					    StringComparison.OrdinalIgnoreCase))
				{
					response.Content.Headers.ContentType =
						System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
							values[0]);

					continue;
				}

				if (!response.Headers.TryAddWithoutValidation(
					    header.Name,
					    values))
				{
					response.Content.Headers.TryAddWithoutValidation(
						header.Name,
						values);
				}
			}
		}


		return response;
	}
}