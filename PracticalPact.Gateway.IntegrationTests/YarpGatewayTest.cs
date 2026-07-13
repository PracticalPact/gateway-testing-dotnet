using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PactNet;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using PracticalPact.Gateway;
using Xunit.Abstractions;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace IntegrationTests;

public class YarpGatewayTest(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task VerifyGateway_YarpGateway_SucceedsWithCorrectContract()
    {
        using var app = CreateYarp(builder =>
        {
            builder.Services.AddGatewayTestingServicesWithYarp();
        });
        app.UseGatewayTesting();

        await app.StartAsync();

        PactVerifierConfig config = new()
        {
            Outputters = [new XunitOutput(outputHelper)],
            LogLevel = PactLogLevel.Debug,
            
        };
        GatewaySetupUtility.VerifyGateway("Gateway", config, app, verifier =>
        {
            return verifier.WithFileSource(new FileInfo(Path.Combine("..", "..", "..", "consumer-contract.json")));
        });

		await app.StopAsync();

		CompareContracts.Compare(new FileInfo(Path.Combine("..", "..", "..", "expected-transformed-contract.json")));
    }

	public static WebApplication CreateYarp(Action<WebApplicationBuilder>? configureBuilder = null)
	{
		var builder = WebApplication.CreateBuilder();

		builder.Services
			.AddReverseProxy()
			.LoadFromMemory(
				routes:
				[
					new()
					{
						RouteId = "route",
						ClusterId = "cluster",
						Match = new()
						{
							Path = "/{**catch-all}"
						}
					}
				],
				clusters:
				[
					new()
					{
						ClusterId = "cluster",
						Destinations = new Dictionary<string, DestinationConfig>
						{
							["destination"] = new()
							{
								Address = "https://example.com/elsewhere/"
							}
						}
					}
				])
				.AddTransforms(builderContext =>
				{
					builderContext.AddRequestTransform(transformContext =>
					{
						if (transformContext.ProxyRequest.Headers.TryGetValues("original-header", out var values))
						{
							transformContext.ProxyRequest.Headers.Remove("original-header");
							transformContext.ProxyRequest.Headers.Add("new-header", values);
						}

						return ValueTask.CompletedTask;
					});
				});

		configureBuilder?.Invoke(builder);

		var app = builder.Build();

		app.MapReverseProxy();

		return app;
	}
}