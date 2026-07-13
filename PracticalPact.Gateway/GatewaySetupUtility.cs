using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using PactNet;
using PactNet.Verifier;
using PracticalPact.Gateway.Execution;
using Yarp.ReverseProxy.Forwarder;

namespace PracticalPact.Gateway;

public static class GatewaySetupUtility
{
	/// <summary>
	/// Adds services required for gateway testing, but without a way to ensure the DelegatingHandler is used. You must do so yourself after calling this.
	/// </summary>
	/// <param name="services">Builder services for gateway app.</param>
	/// <returns></returns>
	public static IServiceCollection AddGatewayTestingServices(this IServiceCollection services)
	{
		services.AddSingleton<ContractBuilderManager>();
		services.AddTransient<CatchTransformedResponseHandler>();
		return services;
	}

	/// <summary>
	/// Adds services required for gateway testing, using a CatchingForwarderHttpClientFactory for YARP.
	/// </summary>
	/// <param name="services">Builder services for gateway app.</param>
	/// <returns></returns>
	public static IServiceCollection AddGatewayTestingServicesWithYarp(this IServiceCollection services)
	{
		services.AddGatewayTestingServices();
		services.AddSingleton<IForwarderHttpClientFactory, CatchingForwarderHttpClientFactory>();
		return services;
	}

	/// <summary>
	/// Adds services required for gateway testing, using a HttpMessageHandlerBuilderFilter.
	/// </summary>
	/// <param name="services">Builder services for gateway app.</param>
	/// <returns></returns>
	public static IServiceCollection AddGatewayTestingServicesWithFilter(this IServiceCollection services)
	{
		services.AddGatewayTestingServices();
		services.AddSingleton<IHttpMessageHandlerBuilderFilter, CatchingHttpMessageHandlerBuilderFilter>();
		services.AddHttpClient();
		return services;
	}

	/// <summary>
	/// Add required middleware and provider state endpoint for gateway testing.
	/// </summary>
	/// <param name="app">Gateway app</param>
	/// <returns></returns>
	public static WebApplication UseGatewayTesting(this WebApplication app)
	{
		app.UseMiddleware<CatchInitialRequestMiddleware>();
		app.MapPost(PactRunner.PROVIDER_STATE_ENDPOINT, (ProviderState providerState, ContractBuilderManager contractBuilderManager) =>
		{
			contractBuilderManager.Builder.MarkProviderState(providerState.State);
		});
		app.UseRouting();
		return app;
	}

	/// <summary>
	/// Run Pact tests for the gateway
	/// </summary>
	/// <param name="gatewayName">Used to deconstruct provider name and construct consumer name. Should match the name of your repository.</param>
	/// <param name="config">Pact config for logging.</param>
	/// <param name="app">Running gateway.</param>
	/// <param name="verifierSourceProducer">Delegate to create verifier source.</param>
	public static void VerifyGateway(string gatewayName, PactVerifierConfig config, WebApplication app, ProduceVerifierSource verifierSourceProducer)
	{
		GatewayPactVerifier verifier = new(gatewayName, app, config);
		verifier.WithHttpEndpoint(new Uri(app.Urls.First()));
		IPactVerifierSource source = verifierSourceProducer.Invoke(verifier);
		source.Verify();
	}

	public delegate IPactVerifierSource ProduceVerifierSource(IPactVerifier pactVerifier);
}