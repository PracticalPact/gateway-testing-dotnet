namespace PracticalPact.Gateway;

public class GatewayNamingUtility(string gatewayName)
{
	public const string NAME_SEPARATOR = "---";

	public bool IsProviderName(string name) => IsProviderName(name, gatewayName);

	public string CreateNewConsumerName(string originalConsumerName) => CreateNewConsumerName(originalConsumerName, gatewayName);

	public static bool IsProviderName(string name, string gatewayName)
	{
		return name.Split(NAME_SEPARATOR)[0] == gatewayName;
	}

	public static string CreateNewConsumerName(string originalConsumerName, string gatewayName)
	{
		return $"{originalConsumerName}{NAME_SEPARATOR}{gatewayName}";
	}

	public static string CreateNewProviderName(string originalProviderName)
	{
		var splitName = originalProviderName.Split(NAME_SEPARATOR);
		return String.Join(NAME_SEPARATOR, splitName.Skip(1));
	}

	public static string CreateConsumerGatewayVersion(string consumerVersion, string gatewayVersion)
	{
		return $"{consumerVersion}{NAME_SEPARATOR}{gatewayVersion}";
	}
}
