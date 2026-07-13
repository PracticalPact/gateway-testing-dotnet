using PracticalPact.Gateway.Execution;

namespace PracticalPact.Gateway.ContractSources;

public sealed class PactBrokerSource(PactBrokerOptions options, ContractFilesHandler filesHandler) : IContractSource
{
    private readonly BrokerClient _client = new();
    public async Task<IEnumerable<ContractFile>> GetContracts(GatewayNamingUtility namingUtility)
    {
        await foreach (string providerName in _client.GetProviderNames(options.BrokerUri, namingUtility))
        {
            await FetchContracts(providerName);
        }

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
        await _client.PublishContract(originalContract, options.BrokerUri, transformation, options.PublishOptions.Version, options.PublishOptions.Branch);
    }
    
    private async Task FetchContracts(string providerName)
    {
        await foreach ((string contract, bool pending) in _client.FetchPacts(options.BrokerUri, providerName, options.ChosenConsumerVersionSelectors, options.PendingEnabled, options.PublishOptions?.Branch))
        {
            filesHandler.StoreContract(contract, pending);
        }
    }
}