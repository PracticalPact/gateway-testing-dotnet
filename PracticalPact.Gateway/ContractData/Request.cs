using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PracticalPact.Gateway.ContractData;

public class Request
{
	public JsonElement? Headers { get; set; }
	public required string Method { get; set; }
	public required string Path { get; set; }
	public JsonElement? Body { get; set; }

	public static Request FromHttpRequest(ReadHttpRequest readRequest)
	{
		HttpRequest request = readRequest.Request;
		var headers = request.Headers.ToDictionary(
			h => h.Key,
			h => h.Value.ToArray());

		return new Request
		{
			Method = request.Method,
			Path = request.Path,
			Headers = JsonSerializer.SerializeToElement(headers),
			Body = readRequest.DeserializedBody
		};
	}

	public Request FromHttpRequestMessage(ReadHttpRequestMessage readRequest)
	{
		HttpRequestMessage request = readRequest.Request;
		var headers = request.Headers.ToDictionary(
			h => h.Key,
			h => h.Value.ToArray());

		if (request.Content != null)
		{
			foreach (var h in request.Content.Headers)
			{
				if (headers.TryGetValue(h.Key, out var existing))
				{
					headers[h.Key] = existing
						.Concat(h.Value)
						.Distinct()
						.ToArray();
				}
				else
				{
					headers[h.Key] = h.Value.ToArray();
				}
			}
		}

		JsonElement? newBody = null;

		if (readRequest.DeserializedBody.HasValue)
		{
			if (Body.HasValue &&
			Body.Value.ValueKind == JsonValueKind.Object)
			{
				var template =
					JsonSerializer.Deserialize<Dictionary<string, object?>>(
						Body.Value.GetRawText())!;

				template["content"] =
					JsonSerializer.Deserialize<object>(
						readRequest.DeserializedBody.Value.GetRawText());

				newBody = JsonSerializer.SerializeToElement(template);
			}
			else
			{
				newBody = JsonSerializer.SerializeToElement(
					new Dictionary<string, object?>
					{
						["content"] =
							JsonSerializer.Deserialize<object>(
								readRequest.DeserializedBody.Value.GetRawText())
					});
			}
		}

		return new Request
		{
			Method = request.Method.Method,
			Path = request.RequestUri?.AbsolutePath ?? "",
			Headers = JsonSerializer.SerializeToElement(headers),
			Body = newBody
		};
	}

	public bool MatchesRequest(ReadHttpRequest request)
	{
		return Matches(FromHttpRequest(request));
	}

	public bool Matches(Request other)
	{
		if (!string.Equals(
			Method,
			other.Method,
			StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (!string.Equals(
			Path,
			other.Path,
			StringComparison.Ordinal))
		{
			return false;
		}

		if (!JsonEquals(ExtractComparableBody(Body), ExtractComparableBody(other.Body)))
		{
			return false;
		}

		if (Headers != null)
		{
			foreach (var headerProperty in Headers.Value.EnumerateObject())
			{
				var headerName = headerProperty.Name;

				if (!other.Headers!.Value.TryGetProperty(
					headerName,
					out var actualHeader))
				{
					return false;
				}

				var expectedValues =
					headerProperty.Value
						.EnumerateArray()
						.Select(x => x.GetString())
						.Where(x => x != null);

				var actualValues =
					actualHeader
						.EnumerateArray()
						.Select(x => x.GetString())
						.Where(x => x != null)
						.ToHashSet(StringComparer.OrdinalIgnoreCase);

				foreach (var expected in expectedValues)
				{
					if (!actualValues.Contains(expected!))
					{
						return false;
					}
				}
			}
		}

		return true;
	}

	private static JsonElement? ExtractComparableBody(JsonElement? body)
	{
		if (!body.HasValue)
		{
			return null;
		}

		var value = body.Value;

		if (value.ValueKind == JsonValueKind.Object &&
			value.TryGetProperty("content", out var content))
		{
			return content;
		}

		return value;
	}

	private static bool JsonEquals(JsonElement? left, JsonElement? right)
	{
		if (!left.HasValue && !right.HasValue)
		{
			return true;
		}

		if (!left.HasValue || !right.HasValue)
		{
			return false;
		}

		return JsonEquals(left.Value, right.Value);
	}

	private static bool JsonEquals(JsonElement left, JsonElement right)
	{
		if (left.ValueKind != right.ValueKind)
		{
			return false;
		}

		switch (left.ValueKind)
		{
			case JsonValueKind.Object:
				{
					var rightProperties =
						right.EnumerateObject()
							 .ToDictionary(x => x.Name);

					foreach (var property in left.EnumerateObject())
					{
						if (!rightProperties.TryGetValue(
							property.Name,
							out var otherProperty))
						{
							return false;
						}

						if (!JsonEquals(
							property.Value,
							otherProperty.Value))
						{
							return false;
						}
					}

					return left.EnumerateObject().Count()
						== right.EnumerateObject().Count();
				}

			case JsonValueKind.Array:
				{
					var leftItems = left.EnumerateArray().ToArray();
					var rightItems = right.EnumerateArray().ToArray();

					if (leftItems.Length != rightItems.Length)
					{
						return false;
					}

					for (int i = 0; i < leftItems.Length; i++)
					{
						if (!JsonEquals(
							leftItems[i],
							rightItems[i]))
						{
							return false;
						}
					}

					return true;
				}

			case JsonValueKind.String:
				return left.GetString() == right.GetString();

			case JsonValueKind.Number:
				return left.GetRawText() == right.GetRawText();

			case JsonValueKind.True:
			case JsonValueKind.False:
				return left.GetBoolean() == right.GetBoolean();

			case JsonValueKind.Null:
			case JsonValueKind.Undefined:
				return true;

			default:
				return left.GetRawText() == right.GetRawText();
		}
	}
}
