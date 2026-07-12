using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PactNet;
using PactNet.Verifier;
using Yarp.ReverseProxy.Forwarder;

namespace PracticalPact.Gateway;

public static class GatewaySetupUtility
{
	public const string PROVIDER_STATE_ENDPOINT = "/provider-states";

	public static IServiceCollection AddGatewayTestingServices(this IServiceCollection services)
	{
		services.AddSingleton<ContractBuilderManager>();
		services.AddTransient<CatchTransformedResponseHandler>();
		services.AddSingleton<IForwarderHttpClientFactory, CatchingForwarderHttpClientFactory>();
		return services;
	}

	public static WebApplication UseGatewayTesting(this WebApplication app)
	{
		app.UseMiddleware<CatchInitialRequestMiddleware>();
		app.MapPost(PROVIDER_STATE_ENDPOINT, (ProviderState providerState, ContractBuilderManager contractBuilderManager) =>
		{
			contractBuilderManager.Builder.MarkProviderState(providerState.State);
		});
		app.UseRouting();
		return app;
	}

	public static void VerifyGateway(string gatewayName, PactVerifierConfig config, WebApplication app, ProduceVerifierSource verifierSourceProducer)
	{
		GatewayPactVerifier verifier = new(gatewayName, app, config);
		verifier.WithHttpEndpoint(new Uri(app.Urls.First()));
		IPactVerifierSource source = verifierSourceProducer.Invoke(verifier);
		source.Verify();
	}

	public delegate IPactVerifierSource ProduceVerifierSource(IPactVerifier pactVerifier);
}