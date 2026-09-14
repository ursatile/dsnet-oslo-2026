using System.Diagnostics;
using Autobarn.ExampleClient;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
	.SetBasePath(AppContext.BaseDirectory)
	.AddJsonFile("appsettings.json", optional: false)
	.AddEnvironmentVariables()
	.Build();

var rootUrl = config["AutobarnApiRootUrl"]
	?? throw new InvalidOperationException("AutobarnApiRootUrl is not configured");

using var http = new HttpClient();
http.BaseAddress = new(rootUrl.EnsureTrailingSlash());
var client = new AutobarnApiClient(http);

while (true) {
	Console.WriteLine("""
   _  _   _ _____ ___  ___   _   ___ _  _
  /_\| | | |_   _/ _ \| _ ) /_\ | _ \ \| |
 / _ \ |_| | | || (_) | _ \/ _ \|   / .` |
/_/ \_\___/  |_| \___/|___/_/ \_\_|_\_|\_|

==========================================

Welcome to the Autobarn API Client!

Available commands:

c:  Reset (clear) all cached data
d:  List all vehicle models
k:  List all vehicle makes
l:  Loop, creating a random vehicle every second until you press any key
r:  Create a random vehicle
x:  Exit
""");

	var key = Console.ReadKey(intercept: true).Key;
	Console.WriteLine();
	var start = Stopwatch.GetTimestamp();
	try {
		// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
		switch (key) {
			case ConsoleKey.K:
				var makes = await client.GetMakesAsync();
				foreach (var make in makes) Console.WriteLine($"{make.Code,-20} {make.Name}");
				Console.WriteLine($"{makes.Count} makes ({Stopwatch.GetElapsedTime(start).TotalMilliseconds:0} ms)");
				break;
			case ConsoleKey.D:
				var models = await client.GetModelsAsync();
				foreach (var (make, makeModels) in models) {
					Console.WriteLine($"{make.Name}:");
					foreach (var model in makeModels) Console.WriteLine($"  {model.Code,-30} {model.Name}");
				}
				Console.WriteLine($"{models.Sum(m => m.Models.Count)} models from {models.Count} makes ({Stopwatch.GetElapsedTime(start).TotalMilliseconds:0} ms)");
				break;
			case ConsoleKey.R:
				await CreateRandomVehicleAsync();
				break;
			case ConsoleKey.L:
				Console.WriteLine("Creating a random vehicle every second. Press any key to stop.");
				while (true) {
					var tick = Stopwatch.GetTimestamp();
					await CreateRandomVehicleAsync();
					while (!Console.KeyAvailable && Stopwatch.GetElapsedTime(tick) < TimeSpan.FromSeconds(1)) await Task.Delay(50);
					if (Console.KeyAvailable) break;
				}
				Console.ReadKey(intercept: true);
				break;
			case ConsoleKey.C:
				client.ClearCache();
				Console.WriteLine("Cache cleared.");
				break;
			case ConsoleKey.X:
				return;
		}
	} catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException or InvalidOperationException) {
		Console.WriteLine($"Error: {ex.Message}");
	}
}

async Task CreateRandomVehicleAsync() {
	var models = await client.GetModelsAsync();
	var modelCodes = models.SelectMany(m => m.Models).Select(m => m.Code).ToList();
	var vehicle = RandomVehicle.Create(modelCodes);
	switch (await client.CreateVehicleAsync(vehicle)) {
		case CreateVehicleResult.Created(var created):
			Console.WriteLine($"Created vehicle {created.Registration}: {created.Year} {created.Color} {vehicle.ModelCode} ({created.FindLinkHref("self")})");
			break;
		case CreateVehicleResult.Rejected(var statusCode, var message):
			Console.WriteLine($"{(int) statusCode} {statusCode}: {message}");
			break;
	}
}

public static class StringExtensions {
	extension(string url) {
		public string EnsureTrailingSlash() => url.TrimEnd('/') + '/';
	}
}
