
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
	public NewVehiclePriceMessage WithPrice(int price, string currencyCode, DateTimeOffset priceCalculatedAt) {
		return new NewVehiclePriceMessage {
			Registration = this.Registration,
			Model = this.Model,
			Make = this.Make,
			Color = this.Color,
			Year = this.Year,
			CreatedAt = this.CreatedAt,
			PriceCalculatedAt = priceCalculatedAt,
			Price = price,
			CurrencyCode = currencyCode
		};
	}	
}

public class NewVehiclePriceMessage : NewVehicleMessage {
	public int Price { get; set; }
	public string CurrencyCode { get; set; } = "";
	public DateTimeOffset PriceCalculatedAt { get; set; }

}

