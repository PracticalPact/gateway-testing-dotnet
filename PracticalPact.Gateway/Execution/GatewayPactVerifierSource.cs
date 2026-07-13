using PactNet.Exceptions;
using PactNet.Verifier;
using PracticalPact.Gateway.ContractSources;

namespace PracticalPact.Gateway.Execution;

public sealed class GatewayPactVerifierSource(PactRunner runner, ContractBuilderManager manager, string gatewayName, IContractSource contractSource) : IPactVerifierSource
{
    private readonly GatewayNamingUtility _namingUtility = new(gatewayName);
    
    public IPactVerifierSource WithProviderStateUrl(Uri providerStateUri)
    {
        throw new NotSupportedException("You may not override the provider state uri as other processes rely on it.");
    }

    public IPactVerifierSource WithProviderStateUrl(Uri providerStateUri, Action<IProviderStateOptions> configure)
    {
        throw new NotSupportedException("You may not override the provider state uri as other processes rely on it.");
    }

    public IPactVerifierSource WithFilter(string? description = null, string? providerState = null)
    {
        throw new NotImplementedException();
    }

    public IPactVerifierSource WithRequestTimeout(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public IPactVerifierSource WithSslVerificationDisabled()
    {
        throw new NotImplementedException();
    }

    public IPactVerifierSource WithCustomHeader(string name, string value)
    {
        throw new NotImplementedException();
    }

    public void Verify()
    {
        VerifyAsync().GetAwaiter().GetResult();
    }

    public async Task VerifyAsync()
    {
        bool anyFailed = false;
        foreach (ContractFile contract in await contractSource.GetContracts(_namingUtility))
        {
            bool verificationSuccess = VerifyContract(contract);
            await contractSource.PostVerification(verificationSuccess, contract.Content, manager.Builder.GetTransformationResult());
            if (!verificationSuccess && !contract.pending)
            {
                anyFailed = true;
            }
        }

        if (anyFailed)
        {
            throw new PactVerificationFailedException("One or more pacts failed verification.");
        }
    }

    private bool VerifyContract(ContractFile contract)
    {
        manager.CreateNewBuilder(contract.Content, _namingUtility);
        try
        {
            runner.RunPact(contract.Path);
            return true;
        }
        catch (PactVerificationFailedException)
        {
            return false;
        }
        
    }
}