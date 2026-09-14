namespace Autobarn.Data.Entities;

///<summary>A model of vehicle built by a particular manufacturer, e.g. the Nissan Note.</summary>
public class VehicleModel {

	///<summary>The code identifying this model, e.g. "nissan-note"</summary>
	public string Code { get; set; } = "";

	///<summary>The part of the code which identifies this model within its make, e.g. "note"</summary>
	public string ModelCode => Code.StartsWith($"{MakeCode}-", StringComparison.OrdinalIgnoreCase)
		? Code[(MakeCode.Length + 1)..]
		: Code;

	public VehicleMake VehicleMake { get; set; } = null!;

	///<summary>The code identifying the manufacturer who builds this model, e.g. "nissan"</summary>
	public string MakeCode { get; set; } = "";

	///<summary>The display name of this model, e.g. "Note"</summary>
	public string Name { get; set; } = "";

	public ICollection<Vehicle> Vehicles { get; set; } = [];
}
