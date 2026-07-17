using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PactNet.Verifier;

namespace PracticalPact.Gateway.Execution;

/// <summary>
/// Handles interactions with the Broker.
/// </summary>
public sealed class BrokerClient : IDisposable
{
	private readonly HttpClient _httpClient = new();

	public async Task<string> FetchPact(string uri)
	{
		var response = await _httpClient.GetAsync(uri);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}

	public async IAsyncEnumerable<(string, bool)> FetchPacts(Uri brokerUri, string providerName, ICollection<ConsumerVersionSelector> consumerVersionSelectors, bool includePending, string? gatewayBranch, DateTime? wipDate)
	{
		var response = await _httpClient.PostAsJsonAsync($"{brokerUri}pacts/provider/{providerName}/for-verification", new FetchPactsDto(consumerVersionSelectors, includePending, gatewayBranch, wipDate?.ToString("yyyy-MM-dd")), FetchPactsDto.serializerOptions);
		response.EnsureSuccessStatusCode();
		var jsonResponse = await response.Content.ReadAsStringAsync();
		var contractEndpoints = JsonDocument.Parse(jsonResponse).RootElement.GetProperty("_embedded").GetProperty("pacts");
		foreach (JsonElement pact in contractEndpoints.EnumerateArray())
		{
			string? uri = pact.GetProperty("_links").GetProperty("self").GetProperty("href").GetString();
			if (uri == null)
			{
				throw new InvalidOperationException("Failed to find contract link from JSON");
			}
			bool pending = pact.GetProperty("verificationProperties").GetProperty("pending").GetBoolean();

			yield return (await FetchPact(uri), pending);
		}
	}

	public async IAsyncEnumerable<string> GetProviderNames(Uri brokerUri, GatewayNamingUtility namingUtility)
	{
		var response = await _httpClient.GetAsync($"{brokerUri}pacticipants");
		response.EnsureSuccessStatusCode();
		var jsonResponse = await response.Content.ReadAsStringAsync();

		foreach (var pacticipant in JsonDocument.Parse(jsonResponse).RootElement.GetProperty("_embedded").GetProperty("pacticipants").EnumerateArray())
		{
			string? name = pacticipant.GetProperty("name").GetString();
			if (name == null || !namingUtility.IsProviderName(name))
			{
				continue;
			}

			yield return name;
		}
	}

	public async Task PublishResult(string contractJson, bool success, string providerVersion, string? providerBranch = null)
	{
		string? endpoint = JsonDocument.Parse(contractJson).RootElement.GetProperty("_links").GetProperty("pb:publish-verification-results").GetProperty("href").GetString();
		if (endpoint == null)
		{
			throw new InvalidOperationException("Contract json did not contain a link to publish results");
		}
		PublishVerificationDto result = new(success, providerVersion);
		var response = await _httpClient.PostAsJsonAsync(endpoint, result);
		response.EnsureSuccessStatusCode();

		if (providerBranch != null)
		{
			string? providerUri = JsonDocument.Parse(contractJson).RootElement.GetProperty("_links").GetProperty("pb:provider").GetProperty("href").GetString();
			if (providerUri == null)
			{
				throw new InvalidOperationException("Contract json did not contain a link to the provider");
			}
			await SetBranch(providerUri, providerVersion, providerBranch);
		}
	}

	public async Task SetBranch(string providerUri, string providerVersion, string providerBranch)
	{
		string endpoint = $"{providerUri}/branches/{providerBranch}/versions/{providerVersion}";
		var response = await _httpClient.PutAsJsonAsync(endpoint, new { });
		response.EnsureSuccessStatusCode();
	}

	public async Task PublishContract(string originalContract, Uri brokerUri, TransformationResult transformation, string gatewayVersion, string? gatewayBranch)
	{
		JsonElement originalConsumerVersionProp = JsonDocument.Parse(originalContract).RootElement.GetProperty("_links").GetProperty("pb:consumer-version");

		string? originalConsumerVersion = originalConsumerVersionProp.GetProperty("name").GetString();
		if (originalConsumerVersion == null)
		{
			throw new InvalidOperationException("Could not find consumer version in original contract");
		}
		string combinedVersion = GatewayNamingUtility.CreateConsumerGatewayVersion(originalConsumerVersion, gatewayVersion);

		string? consumerBranch = await GetOriginalConsumerBranch(originalConsumerVersionProp);

		PublishContractsDto dto = new(transformation.Consumer, combinedVersion, gatewayBranch, [new ContractDto(transformation.Consumer, transformation.Provider, "pact", "application/json", Base64Encode(transformation.Contract))]);
		var response = await _httpClient.PostAsJsonAsync($"{brokerUri}contracts/publish", dto);
		response.EnsureSuccessStatusCode();
	}

	public async Task<string?> GetOriginalConsumerBranch(JsonElement originalConsumerVersionProp)
	{
		try
		{
			string? consumerVersionLink = originalConsumerVersionProp.GetProperty("href").GetString();
			if (consumerVersionLink == null)
			{
				return null;
			}
			var response = await _httpClient.GetAsync(consumerVersionLink);
			response.EnsureSuccessStatusCode();
			string consumerVersionContent = await response.Content.ReadAsStringAsync();
			JsonElement branches = JsonDocument.Parse(consumerVersionContent).RootElement.GetProperty("_embedded").GetProperty("branchVersions");
			foreach (JsonElement branch in branches.EnumerateArray())
			{
				string? branchName = branch.GetProperty("name").GetString();
				return branchName;
			}
		}
		catch (Exception)
		{
			return null;
		}
		return null;
	}

	private static string Base64Encode(string plainText)
	{
		return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
	}

	public void Dispose()
	{
		_httpClient.Dispose();
	}

	private record FetchPactsDto(ICollection<ConsumerVersionSelector> ConsumerVersionSelectors, bool IncludePendingStatus, string? ProviderVersionBranch, string? IncludeWipPactsSince)
	{
		public static readonly JsonSerializerOptions serializerOptions = new()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
		};
	}

	private record PublishVerificationDto(bool success, string providerApplicationVersion);

	private record PublishContractsDto(string pacticipantName, string pacticipantVersionNumber, string? branch, IList<ContractDto> contracts);

	private record ContractDto(string consumerName, string providerName, string specification, string contentType, string content);
}