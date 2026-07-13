using System.Text.Json;
using PracticalPact.Gateway.Execution;

namespace PracticalPact.Gateway.ContractSources;

public sealed class DirectorySource(DirectoryInfo directoryInfo, ContractFilesHandler filesHandler, string[] consumers) : IContractSource
{
    public Task<IEnumerable<ContractFile>> GetContracts(GatewayNamingUtility namingUtility)
    {
        return Task.FromResult(
            directoryInfo
                .EnumerateFiles()
                .Select(c => new ContractFile(c.FullName, File.ReadAllText(c.FullName), false))
                .Where(c => consumers.Contains(JsonDocument.Parse(c.Content).RootElement.GetProperty("consumer").GetProperty("name").GetString())));
    }

    public Task PostVerification(bool success, string originalContract, TransformationResult transformation)
    {
        if (success)
        {
            filesHandler.StoreTransformedContract(transformation);
        }
        return Task.CompletedTask;
    }
}