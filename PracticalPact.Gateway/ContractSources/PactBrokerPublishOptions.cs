using PactNet.Verifier;

namespace PracticalPact.Gateway.ContractSources;

// This file is mostly copied from its equivalent in PactNet at https://github.com/pact-foundation/pact-net/blob/master/src/PactNet/Verifier/PactBrokerPublishOptions.cs

public sealed class PactBrokerPublishOptions : IPactBrokerPublishOptions
{
    public readonly string Version;

    public ICollection<string> Tags { get; private set; } = Array.Empty<string>();
    public string? Branch { get; private set; }
    public Uri? BuildUriToPublish { get; private set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="PactBrokerPublishOptions"/> class.
    /// </summary>
    /// <param name="provider">Pact verifier provider</param>
    /// <param name="version">Provider version</param>
    public PactBrokerPublishOptions(string version)
    {
        Version = version;
    }

    /// <summary>
    /// Tag the provider with the given tags
    /// </summary>
    /// <param name="tags">Tags to apply</param>
    /// <returns>Fluent builder</returns>
    public IPactBrokerPublishOptions ProviderTags(params string[] tags)
    {
        throw new NotImplementedException();
        Tags = tags;
        return this;
    }

    /// <summary>
    /// Set the branch of the provider
    /// </summary>
    /// <param name="branch">Provider branch</param>
    /// <returns>Fluent builder</returns>
    public IPactBrokerPublishOptions ProviderBranch(string branch)
    {
        Branch = branch;
        return this;
    }

    /// <summary>
    /// URI of the build that performed the verification
    /// </summary>
    /// <param name="uri">Build URI</param>
    /// <returns>Fluent builder</returns>
    public IPactBrokerPublishOptions BuildUri(Uri uri)
    {
        throw new NotImplementedException();
        BuildUriToPublish = uri;
        return this;
    }
}