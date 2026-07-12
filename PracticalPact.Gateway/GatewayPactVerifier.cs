using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using PactNet.Verifier;
using PactNet.Verifier.Messaging;
using PracticalPact.Gateway.ContractSources;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PracticalPact.Gateway;

public sealed class GatewayPactVerifier : IPactVerifier
{
	private const string VerifierNotInitialised = $"You must add a verifier transport by calling {nameof(WithHttpEndpoint)}.";
	
	private readonly ContractBuilderManager _manager;
	private readonly string _gatewayName;
	private readonly PactVerifierConfig _config;
	
	private PactRunner? _runner;
	

	public GatewayPactVerifier(string gatewayName, WebApplication app, PactVerifierConfig config)
	{
		_gatewayName = gatewayName;
		_config = config;
		_manager = app.Services.GetRequiredService<ContractBuilderManager>();
	}

	public IPactVerifier WithHttpEndpoint(Uri pactUri)
	{
		_runner = new PactRunner(_config, pactUri);
		return this;
	}

	public IPactVerifier WithMessages(Action<IMessageScenarios> configure)
	{
		throw new NotSupportedException("Gateway testing does not work with Messages.");
	}

	public IPactVerifier WithMessages(Action<IMessageScenarios> configure, JsonSerializerOptions settings)
	{
		throw new NotSupportedException("Gateway testing does not work with Messages.");
	}

	public IPactVerifierSource WithFileSource(FileInfo pactFile)
	{
		return CreateVerifierSource(new FileSource(pactFile, new ContractFilesHandler()));
	}

	public IPactVerifierSource WithDirectorySource(DirectoryInfo directory, params string[] consumers)
	{
		return CreateVerifierSource(new DirectorySource(directory, new ContractFilesHandler(), consumers));
	}

	public IPactVerifierSource WithUriSource(Uri pactUri) => WithUriSource(pactUri, options => { });

	public IPactVerifierSource WithUriSource(Uri pactUri, Action<IPactUriOptions> configure)
	{
		PactUriOptions options = new(pactUri);
		configure.Invoke(options);
		return CreateVerifierSource(new UriSource(options, new ContractFilesHandler()));
	}

	public IPactVerifierSource WithPactBrokerSource(Uri brokerBaseUri) =>
		WithPactBrokerSource(brokerBaseUri, options => { });

	public IPactVerifierSource WithPactBrokerSource(Uri brokerBaseUri, Action<IPactBrokerOptions> configure)
	{
		PactBrokerOptions options = new(brokerBaseUri);
		configure.Invoke(options);
		return CreateVerifierSource(new PactBrokerSource(options, new ContractFilesHandler()));
	}

	[MemberNotNull(nameof(_runner))]
	private void VerifyInitializedRunner()
	{
		if (_runner == null)
		{
			throw new InvalidOperationException(VerifierNotInitialised);
		}
	}

	private GatewayPactVerifierSource CreateVerifierSource(IContractSource contractSource)
	{
		VerifyInitializedRunner();
		return new GatewayPactVerifierSource(_runner, _manager, _gatewayName, contractSource);
	}
}