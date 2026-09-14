using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Autobarn.Website.Models;

/// <summary>
/// Validates that a year is between <see cref="MinimumYear"/> and the current year.
/// Unlike [Range], the upper bound is evaluated when validation runs, so it doesn't go stale.
/// </summary>
public class RegistrationYearAttribute() : ValidationAttribute("{0} must be between {1} and {2}."), IClientModelValidator {

	public const int MinimumYear = 1950;

	public static int MaximumYear => DateTime.Today.Year;

	// null is valid here; use [Required] to reject missing values.
	public override bool IsValid(object? value)
		=> value is not int year || (year >= MinimumYear && year <= MaximumYear);

	public override string FormatErrorMessage(string name)
		=> string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, MinimumYear, MaximumYear);

	public void AddValidation(ClientModelValidationContext context) {
		context.Attributes.TryAdd("data-val", "true");
		context.Attributes.TryAdd("data-val-range", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
		context.Attributes.TryAdd("data-val-range-min", MinimumYear.ToString(CultureInfo.InvariantCulture));
		context.Attributes.TryAdd("data-val-range-max", MaximumYear.ToString(CultureInfo.InvariantCulture));
	}
}
