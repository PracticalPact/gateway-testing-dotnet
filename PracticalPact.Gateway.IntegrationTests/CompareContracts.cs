using System.Text.Json;
using PracticalPact.Gateway.ContractSources;

namespace IntegrationTests;

public static class CompareContracts
{
	public static void Compare(FileInfo expectedContract)
	{
		Compare(File.ReadAllText(expectedContract.FullName));
	}

	public static void Compare(string expectedJson)
	{
		// Prepare data
		using JsonDocument expectedDoc = JsonDocument.Parse(expectedJson);
		string[] transformedFiles = Directory.GetFiles(ContractFilesHandler.TransformedPath);
		if (transformedFiles.Length != 1)
		{
			throw new InvalidOperationException($"Expected one transformed contract at {ContractFilesHandler.TransformedPath}, but found {transformedFiles.Length}.");
		}
		using JsonDocument actualDoc = JsonDocument.Parse(File.ReadAllText(transformedFiles[0]));

		// Compare basic data 
		ElementPair rootPair = new(expectedDoc.RootElement, actualDoc.RootElement);
		rootPair.AssertEqualProperty("consumer");
		rootPair.AssertEqualProperty("provider");
		rootPair.AssertEqualProperty("metadata");

		// Compare interactions
		JsonElement expectedInteractions = rootPair.Expected.GetProperty("interactions");
		JsonElement actualInteractions = rootPair.Actual.GetProperty("interactions");
		if (expectedInteractions.GetArrayLength() != actualInteractions.GetArrayLength())
		{
			throw new ComparisonException($"Contract was expected to have {expectedInteractions.GetArrayLength()}, but has {actualInteractions.GetArrayLength()}.");
		}
		foreach (JsonElement expectedInteraction in expectedInteractions.EnumerateArray())
		{
			AssertMatchingInteraction(expectedInteraction, actualInteractions);
		}
	}

	private static void AssertMatchingInteraction(JsonElement expectedInteraction, JsonElement actualInteractions)
	{
		ElementPair interactionPair = new(expectedInteraction, default);
		foreach (JsonElement actualInteraction in actualInteractions.EnumerateArray())
		{
			interactionPair.Actual = actualInteraction;
			if (interactionPair.CheckEqual())
			{
				return;
			}
		}
		throw new ComparisonException($"Failed to find match for expected interaction with description {expectedInteraction.GetProperty("description").GetString()}");
	}


	struct ElementPair
	{
		public JsonElement Expected { get; set; }
		public JsonElement Actual { get; set; }

		public ElementPair(JsonElement expected, JsonElement actual)
		{
			Expected = expected;
			Actual = actual;
		}

		public void AssertEqualProperty(string propertyName)
		{
			JsonElement expectedProperty;
			JsonElement actualProperty;
			bool hasExpected = Expected.TryGetProperty(propertyName, out expectedProperty);
			bool hasActual = Expected.TryGetProperty(propertyName, out actualProperty);
			if (!hasExpected && !hasActual)
			{
				return;
			}
			if (hasExpected && !hasActual)
			{
				throw new ComparisonException($"Contract is missing property {propertyName}");
			}
			if (!hasExpected && hasActual)
			{
				throw new ComparisonException($"Contract has unexpected property {propertyName}");
			}
			if (!JsonElement.DeepEquals(expectedProperty, actualProperty))
			{
				throw new ComparisonException($"Comparison failed for property {propertyName}");
			}
		}

		public bool CheckEqual()
		{
			return JsonElement.DeepEquals(Expected, Actual);
		}
		
	}

	public class ComparisonException : Exception
	{
		public ComparisonException(string message) : base(message)
		{
			
		}
	}
}
