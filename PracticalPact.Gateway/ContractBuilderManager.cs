namespace PracticalPact.Gateway;

public sealed class ContractBuilderManager
{
	public ContractBuilder Builder
	{
		get
		{
			if (_builder == null)
			{
				throw new Exception("Tried to access builder before tests were started");
			}
			return _builder!;
		}
	}
	private ContractBuilder? _builder;

	public void CreateNewBuilder(string contract, GatewayNamingUtility namingUtility)
	{
		_builder = new ContractBuilder(contract, namingUtility);
	}
}