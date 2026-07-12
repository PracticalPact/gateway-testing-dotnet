using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Forwarder;

namespace PracticalPact.Gateway;

internal class CatchingForwarderHttpClientFactory(IServiceProvider serviceProvider) : ForwarderHttpClientFactory
{
	protected override HttpMessageHandler WrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
	{
		var catchHandler = serviceProvider.GetRequiredService<CatchTransformedResponseHandler>();
		catchHandler.InnerHandler = handler;
		return catchHandler;
	}
}