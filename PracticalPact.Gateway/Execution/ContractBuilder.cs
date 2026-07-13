using PracticalPact.Gateway.ContractData;

namespace PracticalPact.Gateway.Execution;

public sealed class ContractBuilder
{
    private readonly Contract _originalContract;
    private readonly Contract _newContract;
    
    private Interaction? _currentInteraction;
    private string? _currentProviderState;
    
    public ContractBuilder(string initialContract, GatewayNamingUtility namingUtility)
    {
        _originalContract = Contract.DeserializeNew(initialContract);
        _newContract = _originalContract.CreateTransformedCloneBase(namingUtility);
	}

    public void MarkOriginalRequest(ReadHttpRequest request)
    {
        _currentInteraction = _originalContract.GetMatchingInteraction(request, _currentProviderState);
    }

    public HttpResponseMessage MarkTransformedRequest(ReadHttpRequestMessage request)
    {
        if (_currentInteraction == null)
        {
           throw new Exception("Transformed request was marked without first marking an original request");
        }

        Interaction transformedInteraction = _currentInteraction.CreateTransformedClone(request);
        _newContract.Interactions.Add(transformedInteraction);

        _currentInteraction = null;
        _currentProviderState = null;

        return transformedInteraction.Response.CreateResponse();
    }

    public void MarkProviderState(string providerState)
    {
        _currentProviderState = providerState;
    }

    public string GetTransformedContract()
    {
        return _newContract.Serialize();
    }

    public string GetTransformedConsumer()
    {
        return _newContract.Consumer.Name;
    }

    public string GetTransformedProvider() 
    {
        return _newContract.Provider.Name;
    }

    public TransformationResult GetTransformationResult()
    {
        return new TransformationResult(GetTransformedContract(), GetTransformedConsumer(), GetTransformedProvider());
    }
    
}