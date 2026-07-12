namespace PracticalPact.Gateway.ContractSources;

public sealed class FileSource(FileInfo fileInfo, ContractFilesHandler filesHandler) : IContractSource
{
    public async Task<IEnumerable<ContractFile>> GetContracts(GatewayNamingUtility namingUtility)
    {
        return [new ContractFile(fileInfo.FullName, await File.ReadAllTextAsync(fileInfo.FullName), false)];
    }

    public Task PostVerification(bool success, string originalContract, TransformationResult transformation)
    {
        filesHandler.StoreTransformedContract(transformation);
        return Task.CompletedTask;
    }
}