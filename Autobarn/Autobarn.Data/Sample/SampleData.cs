using Autobarn.Data.Entities;
using System.Globalization;

namespace Autobarn.Data.Sample;

public static class SampleData {

	public record CarModelCsvRecord(string Code, string MakeCode, string Name);

	public record CarMakeCsvRecord(string Code, string Name);

	public record VehicleCsvRecord(string Registration, string ModelCode, string Color, int Year);

	public static IEnumerable<CarModelCsvRecord> VehicleModelData
		=> EmbeddedResource.ReadAllLines("models.csv", typeof(SampleData).Assembly)
			.Select(line => line.Split(","))
			.Where(tokens => tokens.Length == 3)
			.Select(tokens => new CarModelCsvRecord(tokens[0], tokens[1], tokens[2]));

	public static IEnumerable<CarMakeCsvRecord> VehicleMakeData
		=> EmbeddedResource.ReadAllLines("makes.csv", typeof(SampleData).Assembly)
			.Select(line => line.Split(","))
			.Where(tokens => tokens.Length == 2)
			.Select(tokens => new CarMakeCsvRecord(tokens[0], tokens[1]));

	public static IEnumerable<Vehicle> Vehicles
		=> EmbeddedResource.ReadCsvData("vehicles.csv", 4)
			.Select(tokens => new Vehicle {
				Registration = tokens[0],
				ModelCode = tokens[1],
				Color = tokens[2],
				Year = int.Parse(tokens[3], CultureInfo.InvariantCulture)
			});
}
