using System.Text.Json;
using PracticalPact.Gateway.Execution;

namespace PracticalPact.Gateway.ContractSources;

public sealed class ContractFilesHandler
{
    public const string BasePath = "pacts_runtime";
	public static readonly string PendingPath = Path.Combine(BasePath, "pending");
    public static readonly string TransformedPath = Path.Combine(BasePath, "transformed");
    
    public ContractFilesHandler()
    {
        EnsureEmptyDirectory();
    }

    public void StoreContract(string content, bool pending)
    {
        string? consumerName = JsonDocument.Parse(content).RootElement.GetProperty("consumer").GetProperty("name").GetString();
        string? providerName = JsonDocument.Parse(content).RootElement.GetProperty("provider").GetProperty("name").GetString();
		string? consumerVersion = JsonDocument.Parse(content).RootElement.GetProperty("_links").GetProperty("pb:consumer-version").GetProperty("name").GetString();
		if (consumerName == null || providerName == null || consumerVersion == null)
        {
            throw new InvalidOperationException("Was unable to get consumer name, provider name, or consumer version from contract json");
        }
		File.WriteAllText(Path.Combine(pending ? PendingPath : BasePath, $"{providerName}---{consumerName}---{consumerVersion}.json"), content);
    }

    public IEnumerable<ContractFile> GetContracts()
    {
        foreach (string contractPath in Directory.GetFiles(BasePath))
        {
            yield return new ContractFile(contractPath, File.ReadAllText(contractPath), false);
        }

		foreach (string contractPath in Directory.GetFiles(PendingPath))
		{
			yield return new ContractFile(contractPath, File.ReadAllText(contractPath), true);
		}
	}

    public void StoreTransformedContract(TransformationResult transformation)
    {
		File.WriteAllText(Path.Combine(TransformedPath, $"{transformation.Consumer}-TO-{transformation.Provider}.json"), transformation.Contract);
    }

    private void EnsureEmptyDirectory()
    {
        if (Directory.Exists(BasePath))
        {
            Directory.Delete(BasePath, true);
        }

        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(PendingPath);
        Directory.CreateDirectory(TransformedPath);
    }
}