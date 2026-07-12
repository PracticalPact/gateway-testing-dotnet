using PactNet;
using PactNet.Verifier;
using PracticalPact.Gateway;
using PactNet.Output.Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public class UnitTest1(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Test1()
    {
        var app = MinimalGateway.Create(builder =>
        {
            builder.Services.AddGatewayTestingServices();
        });
        app.UseGatewayTesting();

        await app.StartAsync();

        PactVerifierConfig config = new()
        {
            Outputters = [new XunitOutput(outputHelper)],
            LogLevel = PactLogLevel.Trace,
            
        };
        GatewaySetupUtility.VerifyGateway("Gateway", config, app, verifier =>
        {
            return verifier.WithFileSource(new FileInfo(Path.Combine("..", "..", "..", "consumer-contract.json")));
        });
    }
}