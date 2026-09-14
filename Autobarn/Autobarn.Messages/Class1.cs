
namespace Autobarn.Messages;

public class NewVehicleMessage {
	public string Registration { get; set; } = "missing";
	public string Model { get; set; } = "missing";
	public string Make { get; set; } = "missing";
	public string Color { get; set; } = "missing";
	public int Year { get; set; }

	public DateTimeOffset CreatedAt { get; set; }
	public override string ToString() {
		return $"New Vehicle: {Registration} ({Make} {Model}, {Color}, {Year}) at {CreatedAt}";
	}
}
