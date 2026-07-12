namespace PracticalPact.Gateway.ContractSources;

public interface IContractSource
{
    Task<IEnumerable<ContractFile>> GetContracts(GatewayNamingUtility namingUtility);

    Task PostVerification(bool success, string originalContract, TransformationResult transformation);
}