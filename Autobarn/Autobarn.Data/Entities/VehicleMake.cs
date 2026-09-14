namespace Autobarn.Data.Entities;

///<summary>A manufacturer who builds vehicles, e.g. Nissan.</summary>
public class VehicleMake {

	///<summary>The code identifying this manufacturer, e.g. "nissan"</summary>
	public string Code { get; set; } = "";

	///<summary>The display name of this manufacturer, e.g. "Nissan"</summary>
	public string Name { get; set; } = "";

	public ICollection<VehicleModel> Models { get; set; } = [];
}
