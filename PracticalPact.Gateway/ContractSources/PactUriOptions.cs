using PactNet.Verifier;

namespace PracticalPact.Gateway.ContractSources;

// This file is mostly copied from its equivalent in PactNet at https://github.com/pact-foundation/pact-net/blob/master/src/PactNet/Verifier/PactUriOptions.cs
internal class PactUriOptions(Uri pactUri) : IPactUriOptions
{
	public Uri PactUri { get; } = pactUri;
	public PactBrokerPublishOptions? PublishOptions { get; private set; }

	public IPactUriOptions BasicAuthentication(string username, string password)
	{
		throw new NotImplementedException();
	}

	public IPactUriOptions PublishResults(string providerVersion) => PublishResults(providerVersion, _ => { });

	public IPactUriOptions PublishResults(string providerVersion, Action<IPactBrokerPublishOptions> configure)
	{
		PublishOptions = new PactBrokerPublishOptions(providerVersion);
		configure.Invoke(PublishOptions);
		return this;
	}

	public IPactUriOptions PublishResults(bool condition, string providerVersion) => PublishResults(condition, providerVersion, _ => { });

	public IPactUriOptions PublishResults(bool condition, string providerVersion, Action<IPactBrokerPublishOptions> configure) => 
		condition ? PublishResults(providerVersion, configure) : this;

	public IPactUriOptions TokenAuthentication(string token)
	{
		throw new NotImplementedException();
	}
}
