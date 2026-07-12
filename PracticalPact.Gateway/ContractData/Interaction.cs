using System.Text.Json;

namespace PracticalPact.Gateway.ContractData;

public sealed class Interaction
{
    public required string Description { get; set; }
    public List<ProviderState> ProviderStates { get; set; } = [];
    public required Request Request { get; set; }
    public required Response Response { get; set; }
    public string? Type { get; set; }

    public bool MatchesRequest(ReadHttpRequest httpRequest, string? providerState)
    {
        return MatchesProviderState(providerState) && Request.MatchesRequest(httpRequest);
    }

    private bool MatchesProviderState(string? providerState)
    {
        if (providerState == null){
            return ProviderStates.Count == 0;
        }
        return ProviderStates.Any(ps => ps.Name == providerState);
	}

    public Interaction CreateTransformedClone(ReadHttpRequestMessage httpRequestMessage)
    {
        return new Interaction()
        {
            Description = Description,
            ProviderStates = ProviderStates,
            Request = Request.FromHttpRequestMessage(httpRequestMessage),
            Response = Response,
            Type = Type
        };

    }

	public override string ToString()
	{
        return JsonSerializer.Serialize(this);
	}
}