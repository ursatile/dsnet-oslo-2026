using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Autobarn.ExampleClient.Resources;

namespace Autobarn.ExampleClient;

public abstract record CreateVehicleResult {
	public record Created(Vehicle Vehicle) : CreateVehicleResult;
	public record Rejected(HttpStatusCode StatusCode, string Message) : CreateVehicleResult;
}

public class AutobarnApiClient(HttpClient http) {

	private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

	private ApiRoot? apiRoot;
	private List<VehicleMake>? makes;
	private List<(VehicleMake Make, List<VehicleModel> Models)>? models;

	public async Task<List<VehicleMake>> GetMakesAsync(CancellationToken cancellationToken = default) {
		if (makes is not null) return makes;
		var href = (await GetApiRootAsync(cancellationToken)).FindLinkHref("makes")
			?? throw new InvalidOperationException("API root has no 'makes' link");
		return makes = await GetAllPagesAsync<VehicleMake>(href, cancellationToken);
	}

	public async Task<List<(VehicleMake Make, List<VehicleModel> Models)>> GetModelsAsync(CancellationToken cancellationToken = default) {
		if (models is not null) return models;
		var result = new List<(VehicleMake, List<VehicleModel>)>();
		foreach (var make in await GetMakesAsync(cancellationToken)) {
			var href = make.FindLinkHref("models") ?? throw new InvalidOperationException($"Make '{make.Code}' has no 'models' link");
			result.Add((make, await GetAllPagesAsync<VehicleModel>(href, cancellationToken)));
		}
		return models = result;
	}

	public async Task<CreateVehicleResult> CreateVehicleAsync(NewVehicle vehicle, CancellationToken cancellationToken = default) {
		var href = (await GetApiRootAsync(cancellationToken)).FindLinkHref("vehicles")
			?? throw new InvalidOperationException("API root has no 'vehicles' link");
		Console.WriteLine($"POST {new Uri(http.BaseAddress!, href)}");
		using var response = await http.PostAsJsonAsync(href, vehicle, jsonOptions, cancellationToken);
		switch (response.StatusCode) {
			case HttpStatusCode.Created:
				var created = await response.Content.ReadFromJsonAsync<Vehicle>(jsonOptions, cancellationToken)
					?? throw new InvalidOperationException($"Empty response from POST {href}");
				return new CreateVehicleResult.Created(created);
			case HttpStatusCode.BadRequest:
			case HttpStatusCode.Conflict:
				return new CreateVehicleResult.Rejected(response.StatusCode, await ReadErrorMessageAsync(response, cancellationToken));
			default:
				response.EnsureSuccessStatusCode();
				throw new HttpRequestException($"Unexpected response from POST {href}: {(int) response.StatusCode} {response.ReasonPhrase}");
		}
	}

	public void ClearCache() {
		apiRoot = null;
		makes = null;
		models = null;
	}

	private async Task<ApiRoot> GetApiRootAsync(CancellationToken cancellationToken)
		=> apiRoot ??= await GetAsync<ApiRoot>(http.BaseAddress!.ToString(), cancellationToken);

	private async Task<List<T>> GetAllPagesAsync<T>(string href, CancellationToken cancellationToken) {
		var items = new List<T>();
		var visited = new HashSet<string>();
		var next = href;
		while (next is not null) {
			if (!visited.Add(next)) throw new InvalidOperationException($"Pagination loop: {next} was already fetched");
			var page = await GetAsync<Page<T>>(next, cancellationToken);
			items.AddRange(page.Items);
			next = page.FindLinkHref("next");
		}
		return items;
	}

	private async Task<T> GetAsync<T>(string href, CancellationToken cancellationToken) {
		// Links may be absolute or relative; HttpClient resolves relative links against BaseAddress.
		Console.WriteLine($"GET {new Uri(http.BaseAddress!, href)}");
		return await http.GetFromJsonAsync<T>(href, jsonOptions, cancellationToken)
			?? throw new InvalidOperationException($"Empty response from {href}");
	}

	// The API returns errors either as a JSON string, e.g. "Model with code 'x' not found.",
	// or as a validation problem details object; anything else is displayed as-is.
	private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
		var body = await response.Content.ReadAsStringAsync(cancellationToken);
		try {
			using var json = JsonDocument.Parse(body);
			var root = json.RootElement;
			if (root.ValueKind == JsonValueKind.String) return root.GetString()!;
			if (root.ValueKind == JsonValueKind.Object) {
				if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object) {
					return String.Join(" ", errors.EnumerateObject()
						.SelectMany(error => error.Value.EnumerateArray().Select(message => message.GetString())));
				}
				if (root.TryGetProperty("title", out var title)) return title.GetString() ?? body;
			}
		} catch (JsonException) {
			// not JSON; fall through and return the raw body
		}
		return body;
	}
}
