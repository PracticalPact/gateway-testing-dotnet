namespace PracticalPact.Gateway.ContractSources;

public interface IContractRepository
{
    void StoreContract(string content);

    IEnumerable<string> GetContracts();
}