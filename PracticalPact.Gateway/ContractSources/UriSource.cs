namespace PracticalPact.Gateway.ContractSources;

internal class UriSource(PactUriOptions options, ContractFilesHandler filesHandler) : IContractSource
{
	private readonly BrokerClient _client = new();

	public async Task<IEnumerable<ContractFile>> GetContracts(GatewayNamingUtility namingUtility)
	{
		string contract = await _client.FetchPact(options.PactUri.ToString());
		filesHandler.StoreContract(contract, false);
		return filesHandler.GetContracts();
	}

	public async Task PostVerification(bool success, string originalContract, TransformationResult transformation)
	{
		if (options.PublishOptions == null)
		{
			return;
		}
		await _client.PublishResult(originalContract, success, options.PublishOptions.Version, options.PublishOptions.Branch);

		if (!success)
		{
			return;
		}
		await _client.PublishContract(originalContract, ExtractBrokerUri(), transformation, options.PublishOptions.Version, options.PublishOptions.Branch);

	}

	private Uri ExtractBrokerUri()
	{
		string[] segments = options.PactUri.Segments;

		// Expect:
		// /pacts/provider/{provider}/consumer/{consumer}/version/{version}/metadata/{metadata}
		const int pactSuffixLength = 9;

		if (segments.Length < pactSuffixLength)
		{
			throw new InvalidOperationException("Pact URI is shorter than expected.");
		}

		int relevantSegmentsCount = segments.Length - pactSuffixLength;

		if (!segments[relevantSegmentsCount].TrimEnd('/').Equals("pacts", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("Expected Pact URI format: {broker}/pacts/provider/{provider}/consumer/{consumer}/version/{version}/metadata/{metadata}");
		}

		string brokerPath = string.Concat(segments.Take(relevantSegmentsCount));
		return new Uri(options.PactUri.GetLeftPart(UriPartial.Authority) + brokerPath);
	}
}