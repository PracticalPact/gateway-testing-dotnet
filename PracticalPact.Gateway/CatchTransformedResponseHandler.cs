using PracticalPact.Gateway.ContractData;

namespace PracticalPact.Gateway;

public sealed class CatchTransformedResponseHandler(ContractBuilderManager contractBuilderManager) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		ReadHttpRequestMessage readRequest = await ReadHttpRequestMessage.Create(request);
		return contractBuilderManager.Builder.MarkTransformedRequest(readRequest);
	}
}