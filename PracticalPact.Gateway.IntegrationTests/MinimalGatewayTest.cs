using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PactNet;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using PracticalPact.Gateway;
using Xunit.Abstractions;

namespace IntegrationTests;

public sealed class MinimalGatewayTest(ITestOutputHelper outputHelper)
{
	[Fact]
	public async Task VerifyGateway_MinimalGateway_Succeeds()
	{
		using var app = CreateMinimal(builder =>
		{
			builder.Services.AddGatewayTestingServicesWithFilter();
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
	}

	public static WebApplication CreateMinimal(Action<WebApplicationBuilder>? configureBuilder = null)
    {
        var builder = WebApplication.CreateBuilder();
        configureBuilder?.Invoke(builder);
        var app = builder.Build();
		
        app.Map("/{**path}", async (IHttpClientFactory factory, HttpContext context) =>
        {
			var client = factory.CreateClient();
            var response = await client.GetAsync("https://example.com");
            var content = await response.Content.ReadAsStringAsync();
            
            context.Response.StatusCode = (int)response.StatusCode;
            
            foreach (var header in response.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in response.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
			await response.Content.CopyToAsync(context.Response.Body);
		});
        
        return app;
    }
}
