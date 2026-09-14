using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Autobarn.Website.Models;

public partial class VehicleDto {

	///<summary>The code identifying the make &amp; model, e.g. "dmc-delorean"</summary>
	[HiddenInput(DisplayValue = false)]
	public string? ModelCode { get; set; }

	public string? ModelName { get; set; }

	[GeneratedRegex("[^A-Z0-9]")]
	private static partial Regex VehicleRegistrationRegex();

	///<summary>The registration code that identifies this vehicle, e.g. "OUTATIME"</summary>
	[Required]
	[DisplayName("Registration Plate")]
	public string? Registration {
		get;
		set => field = value is null ? null : VehicleRegistrationRegex().Replace(value.ToUpperInvariant(), "");
	}

	///<summary>The year this vehicle was registered, e.g. 1985</summary>
	[Required]
	[DisplayName("Year of first registration")]
	[RegistrationYear]
	public int? Year { get; set; }

	///<summary>What color is this vehicle, e.g. "Silver"</summary>
	[Required]
	[DisplayName("Colour")]
	public string Color { get; set; } = "";

	public static IReadOnlyList<string> Colors { get; } = [
		"Black", "Blue", "Gold", "Green", "Grey", "Orange",
		"Purple", "Red", "Silver", "Turquoise", "White", "Yellow"
	];
}
