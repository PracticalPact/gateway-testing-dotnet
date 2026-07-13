using PactNet.Verifier;

namespace PracticalPact.Gateway.Execution;

public class PactRunner(PactVerifierConfig config, Uri appUrl)
{
	public const string PROVIDER_STATE_ENDPOINT = "/provider-states";

	public void RunPact(string pactPath)
	{
		IPactVerifier pactVerifier = new PactVerifier("", config);
		pactVerifier.WithHttpEndpoint(appUrl);
			
		IPactVerifierSource verifierSource = pactVerifier.WithFileSource(new FileInfo(pactPath));

		verifierSource.WithProviderStateUrl(new Uri(appUrl, PROVIDER_STATE_ENDPOINT));

		verifierSource.Verify();
	}
}