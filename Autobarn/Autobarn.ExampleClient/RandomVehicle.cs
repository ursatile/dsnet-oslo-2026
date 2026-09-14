using Autobarn.ExampleClient.Resources;

namespace Autobarn.ExampleClient;

public static class RandomVehicle {

	private const string REGISTRATION_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
	private const int REGISTRATION_LENGTH = 8;
	private const int MIN_YEAR = 1960;
	private const int MAX_YEAR = 2025;

	// https://developer.mozilla.org/en-US/docs/Web/CSS/named-color
	private static readonly string[] colors = [
		"aliceblue", "antiquewhite", "aqua", "aquamarine", "azure", "beige", "bisque", "black", "blanchedalmond",
		"blue", "blueviolet", "brown", "burlywood", "cadetblue", "chartreuse", "chocolate", "coral", "cornflowerblue",
		"cornsilk", "crimson", "cyan", "darkblue", "darkcyan", "darkgoldenrod", "darkgray", "darkgreen", "darkgrey",
		"darkkhaki", "darkmagenta", "darkolivegreen", "darkorange", "darkorchid", "darkred", "darksalmon",
		"darkseagreen", "darkslateblue", "darkslategray", "darkslategrey", "darkturquoise", "darkviolet", "deeppink",
		"deepskyblue", "dimgray", "dimgrey", "dodgerblue", "firebrick", "floralwhite", "forestgreen", "fuchsia",
		"gainsboro", "ghostwhite", "gold", "goldenrod", "gray", "green", "greenyellow", "grey", "honeydew", "hotpink",
		"indianred", "indigo", "ivory", "khaki", "lavender", "lavenderblush", "lawngreen", "lemonchiffon", "lightblue",
		"lightcoral", "lightcyan", "lightgoldenrodyellow", "lightgray", "lightgreen", "lightgrey", "lightpink",
		"lightsalmon", "lightseagreen", "lightskyblue", "lightslategray", "lightslategrey", "lightsteelblue",
		"lightyellow", "lime", "limegreen", "linen", "magenta", "maroon", "mediumaquamarine", "mediumblue",
		"mediumorchid", "mediumpurple", "mediumseagreen", "mediumslateblue", "mediumspringgreen", "mediumturquoise",
		"mediumvioletred", "midnightblue", "mintcream", "mistyrose", "moccasin", "navajowhite", "navy", "oldlace",
		"olive", "olivedrab", "orange", "orangered", "orchid", "palegoldenrod", "palegreen", "paleturquoise",
		"palevioletred", "papayawhip", "peachpuff", "peru", "pink", "plum", "powderblue", "purple", "rebeccapurple",
		"red", "rosybrown", "royalblue", "saddlebrown", "salmon", "sandybrown", "seagreen", "seashell", "sienna",
		"silver", "skyblue", "slateblue", "slategray", "slategrey", "snow", "springgreen", "steelblue", "tan", "teal",
		"thistle", "tomato", "turquoise", "violet", "wheat", "white", "whitesmoke", "yellow", "yellowgreen"
	];

	public static NewVehicle Create(IReadOnlyList<string> modelCodes) {
		if (modelCodes.Count == 0) throw new InvalidOperationException("No vehicle models available to choose from");
		return new(
			Registration: Random.Shared.GetString(REGISTRATION_CHARS, REGISTRATION_LENGTH),
			ModelCode: modelCodes[Random.Shared.Next(modelCodes.Count)],
			Year: Random.Shared.Next(MIN_YEAR, MAX_YEAR + 1),
			Color: colors[Random.Shared.Next(colors.Length)]
		);
	}
}
