using System.Text.Json;
using System.Text.Json.Serialization;
using PactNet.Verifier;

namespace PracticalPact.Gateway.ContractSources;

// This file is mostly copied from its equivalent in PactNet at https://github.com/pact-foundation/pact-net/blob/master/src/PactNet/Verifier/PactBrokerOptions.cs

public class PactBrokerOptions : IPactBrokerOptions
{
        private static readonly JsonSerializerOptions ConsumerSelectorSettings = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };
        
        public readonly Uri BrokerUri;
        
        public PactBrokerPublishOptions? PublishOptions { get; private set; }

        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public string? Token { get; private set; }
        public bool PendingEnabled { get; private set; }
        public DateTime? WipPactsIncludedSince { get; private set; }
        public string? ProviderBranchFilter { get; private set; }
        public ICollection<string> ChosenProviderTags  { get; private set; } = Array.Empty<string>();
        public ICollection<string> ChosenConsumerVersionTags  { get; private set; } = Array.Empty<string>();
        public ICollection<ConsumerVersionSelector> ChosenConsumerVersionSelectors  { get; private set; } = Array.Empty<ConsumerVersionSelector>();

        /// <summary>
        /// Initialises a new instance of the <see cref="PactBrokerOptions"/> class.
        /// </summary>
        /// <param name="brokerUri">Pact broker URI</param>
        public PactBrokerOptions(Uri brokerUri)
        {
            BrokerUri = brokerUri;
        }

        /// <summary>
        /// Use Basic authentication with the Pact Broker
        /// </summary>
        /// <param name="username">Pact broker username</param>
        /// <param name="password">Pact broker password</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions BasicAuthentication(string username, string password)
        {
            throw new NotImplementedException();
            Username = username;
            Password = password;
            return this;
        }

        /// <summary>
        /// Use Token authentication with the Pact Broker
        /// </summary>
        /// <param name="token">Auth token</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions TokenAuthentication(string token)
        {
            throw new NotImplementedException();
            Token = token;
            return this;
        }

        /// <summary>
        /// Enable pending pacts
        /// </summary>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions EnablePending()
        {
            PendingEnabled = true;
            return this;
        }

        /// <summary>
        /// Set the provider branch for retrieving pacts
        /// </summary>
        /// <param name="branch">Branch name</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions ProviderBranch(string branch)
        {
            ProviderBranchFilter = branch;
            return this;
        }

        /// <summary>
        /// Set the provider tags for retrieving pacts
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions ProviderTags(params string[] tags)
        {
            throw new NotImplementedException();
            ChosenProviderTags = tags;
            return this;
        }

        /// <summary>
        /// Consumer tag versions to retrieve
        /// </summary>
        /// <param name="tags">Consumer tags</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions ConsumerTags(params string[] tags)
        {
            throw new NotImplementedException();
            ChosenConsumerVersionTags = tags;
            return this;
        }

        /// <summary>
        /// Consumer version selectors to control which pacts are returned from the broker
        /// </summary>
        /// <param name="selectors">Consumer version selectors</param>
        /// <returns>Fluent builder</returns>
        /// <remarks>See <see href="https://docs.pact.io/pact_broker/advanced_topics/consumer_version_selectors"/></remarks>
        public IPactBrokerOptions ConsumerVersionSelectors(ICollection<ConsumerVersionSelector> selectors)
        {
            ChosenConsumerVersionSelectors = selectors;
            return this;
        }

        /// <summary>
        /// Consumer version selectors to control which pacts are returned from the broker
        /// </summary>
        /// <param name="selectors">Consumer version selectors</param>
        /// <returns>Fluent builder</returns>
        /// <remarks>See <see href="https://docs.pact.io/pact_broker/advanced_topics/consumer_version_selectors"/></remarks>
        public IPactBrokerOptions ConsumerVersionSelectors(params ConsumerVersionSelector[] selectors)
            => ConsumerVersionSelectors((ICollection<ConsumerVersionSelector>)selectors);

        /// <summary>
        /// Include WIP pacts since the given date
        /// </summary>
        /// <param name="date">WIP cut-off date</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions IncludeWipPactsSince(DateTime date)
        {
            WipPactsIncludedSince = date;
            return this;
        }

        /// <summary>
        /// Publish results to the pact broker without any additional settings
        /// </summary>
        /// <param name="providerVersion">Provider version</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions PublishResults(string providerVersion)
            => PublishResults(providerVersion, _ => { });

        /// <summary>
        /// Publish results to the pact broker
        /// </summary>
        /// <param name="providerVersion">Provider version</param>
        /// <param name="configure">Configure the publish options</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions PublishResults(string providerVersion, Action<IPactBrokerPublishOptions> configure)
        {
            PublishOptions = new PactBrokerPublishOptions(providerVersion);
            configure.Invoke(PublishOptions);
            return this;
        }

        /// <summary>
        /// Publish results to the pact broker without any additional settings, if the condition is met
        /// </summary>
        /// <param name="condition">Only publish if this condition is true</param>
        /// <param name="providerVersion">Provider version</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions PublishResults(bool condition, string providerVersion)
            => PublishResults(condition, providerVersion, _ => { });

        /// <summary>
        /// Publish results to the pact broker if the condition is met
        /// </summary>
        /// <param name="condition">Only publish if this condition is true</param>
        /// <param name="providerVersion">Provider version</param>
        /// <param name="configure">Configure the publish options</param>
        /// <returns>Fluent builder</returns>
        public IPactBrokerOptions PublishResults(bool condition, string providerVersion, Action<IPactBrokerPublishOptions> configure)
            => condition ? PublishResults(providerVersion, configure) : this;
}