using System.Text.Json.Serialization;

namespace Autobarn.ExampleClient.Resources;

public record Hyperlink(string Href);

public abstract record Resource(
	[property: JsonPropertyName("_links")]
	Dictionary<string, Hyperlink> Links) {
	public string? FindLinkHref(string rel) => Links.GetValueOrDefault(rel)?.Href;
}

public record ApiRoot(Dictionary<string, Hyperlink> Links) : Resource(Links);

public record VehicleMake(Dictionary<string, Hyperlink> Links, string Code, string Name) : Resource(Links);

public record VehicleModel(Dictionary<string, Hyperlink> Links, string Code, string Name) : Resource(Links);

public record Vehicle(Dictionary<string, Hyperlink> Links, string Registration, int Year, string Color) : Resource(Links);

public record Page<T>(Dictionary<string, Hyperlink> Links, int Index, int Count, int Total, List<T> Items) : Resource(Links);

public record NewVehicle(string Registration, string ModelCode, int Year, string Color);
