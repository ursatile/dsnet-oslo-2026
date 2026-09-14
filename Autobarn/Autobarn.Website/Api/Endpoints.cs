namespace Autobarn.Website.Api;

/// <summary>Names of the Autobarn API endpoints, used to generate hypermedia links.</summary>
public static class Endpoints {
	public const string GET_API_ROOT = nameof(GET_API_ROOT);
	public const string GET_VEHICLES = nameof(GET_VEHICLES);
	public const string GET_VEHICLE = nameof(GET_VEHICLE);
	public const string GET_MAKES = nameof(GET_MAKES);
	public const string GET_MAKE = nameof(GET_MAKE);
	public const string GET_MODELS_BY_MAKE = nameof(GET_MODELS_BY_MAKE);
	public const string GET_MODEL = nameof(GET_MODEL);
	public const string GET_VEHICLES_BY_MODEL = nameof(GET_VEHICLES_BY_MODEL);
	public const string POST_VEHICLE = nameof(POST_VEHICLE);
}
