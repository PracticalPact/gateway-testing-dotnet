using System.Text.Json;

namespace PracticalPact.Gateway.ContractSources;

public sealed class ContractFilesHandler
{
    private const string _path = "pacts_runtime";
    private readonly string _pendingPath = _path + "/pending";
    private readonly string _transformedPath = _path + "/transformed";
    
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
        File.WriteAllText(Path.Combine(pending ? _pendingPath : _path, $"{providerName}---{consumerName}---{consumerVersion}.json"), content);
    }

    public IEnumerable<ContractFile> GetContracts()
    {
        foreach (string contractPath in Directory.GetFiles(_path))
        {
            yield return new ContractFile(contractPath, File.ReadAllText(contractPath), false);
        }

		foreach (string contractPath in Directory.GetFiles(_pendingPath))
		{
			yield return new ContractFile(contractPath, File.ReadAllText(contractPath), true);
		}
	}

    public void StoreTransformedContract(TransformationResult transformation)
    {
        File.WriteAllText(Path.Combine(_transformedPath, $"{transformation.Consumer}-TO-{transformation.Provider}.json"), transformation.Contract);
    }

    private void EnsureEmptyDirectory()
    {
        if (Directory.Exists(_path))
        {
            Directory.Delete(_path, true);
        }

        Directory.CreateDirectory(_path);
        Directory.CreateDirectory(_pendingPath);
        Directory.CreateDirectory(_transformedPath);
    }
}