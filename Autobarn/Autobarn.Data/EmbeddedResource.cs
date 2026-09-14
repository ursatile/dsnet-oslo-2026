using System.Reflection;

namespace Autobarn.Data;

public static class EmbeddedResource {

	public static Stream OpenStream(string resourceFileName, Assembly? assembly = null) {
		assembly ??= typeof(EmbeddedResource).Assembly;
		var name = assembly.GetManifestResourceNames()
			.FirstOrDefault(n => n.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase));
		return (name is null ? null : assembly.GetManifestResourceStream(name))
			?? throw new FileNotFoundException($"Embedded resource not found in {assembly.GetName().Name}", resourceFileName);
	}

	public static byte[] ReadBytes(string resourceFileName, Assembly? assembly = null) {
		using var stream = OpenStream(resourceFileName, assembly);
		using var ms = new MemoryStream();
		stream.CopyTo(ms);
		return ms.ToArray();
	}

	public static string ReadAllText(string resourceFileName, Assembly? assembly = null) {
		using var reader = new StreamReader(OpenStream(resourceFileName, assembly));
		return reader.ReadToEnd();
	}

	public static string[] ReadAllLines(string resourceFileName, Assembly? assembly = null)
		=> ReadAllText(resourceFileName, assembly).ReplaceLineEndings().Split(Environment.NewLine);

	/// <summary>Reads a simple (unquoted) CSV resource, skipping any line that doesn't have exactly <paramref name="columns"/> fields.</summary>
	public static IEnumerable<string[]> ReadCsvData(string resourceFileName, int columns, Assembly? assembly = null)
		=> ReadAllLines(resourceFileName, assembly)
			.Select(line => line.Split(','))
			.Where(items => items.Length == columns);

}
