using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace PracticalPact.Gateway.Execution;

public sealed class CatchingHttpMessageHandlerBuilderFilter(IServiceProvider serviceProvider) : IHttpMessageHandlerBuilderFilter
{
	public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
	{
		return builder =>
		{
			next(builder);
			builder.AdditionalHandlers.Add(serviceProvider.GetRequiredService<CatchTransformedResponseHandler>());
		};
	}
}
