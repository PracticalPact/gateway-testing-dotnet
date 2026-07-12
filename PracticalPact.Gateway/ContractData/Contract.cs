using System.Text.Json;

namespace PracticalPact.Gateway.ContractData;

public sealed class Contract
{
    public required Consumer Consumer { get; set; }
    public required IList<Interaction> Interactions { get; set; }
    public required JsonElement Metadata { get; set; }
    public required Provider Provider { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static Contract DeserializeNew(string jsonContract)
    {
        Contract? newContract = JsonSerializer.Deserialize<Contract>(jsonContract, JsonOptions);
        if (newContract == null)
        {
            throw new Exception("Failed to read contract");
        }

        return newContract;
    }

    public string Serialize(){
		return JsonSerializer.Serialize(this, JsonOptions);
    }

    public Interaction GetMatchingInteraction(ReadHttpRequest httpRequest, string? providerState)
    {
        foreach (Interaction interaction in Interactions)
        {
            if (interaction.MatchesRequest(httpRequest, providerState)){
                return interaction;
            }
        }
        throw new Exception($"Found no matching interaction for\n{httpRequest.ToString()}\n\n{providerState}");
    }

    public Contract CreateTransformedCloneBase(GatewayNamingUtility namingUtility){
        return new Contract()
        {
            Consumer = new Consumer() { Name = namingUtility.CreateNewConsumerName(Consumer.Name) },
            Interactions = [],
            Metadata = Metadata,
            Provider = new Provider() { Name = GatewayNamingUtility.CreateNewProviderName(Provider.Name) }
        };
    }
}