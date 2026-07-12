using PactNet.Verifier;

namespace PracticalPact.Gateway;

public class PactRunner(PactVerifierConfig config, Uri appUrl)
{
	public void RunPact(string pactPath)
	{
		IPactVerifier pactVerifier = new PactVerifier("FavoriteApi", config);
		pactVerifier.WithHttpEndpoint(appUrl);
			
		IPactVerifierSource verifierSource = pactVerifier.WithFileSource(new FileInfo(pactPath));

		verifierSource.WithProviderStateUrl(new Uri(appUrl, GatewaySetupUtility.PROVIDER_STATE_ENDPOINT));

		verifierSource.Verify();
	}
}