namespace Autobarn.Data.Entities;

///<summary>A specific vehicle which is listed for sale at Autobarn.</summary>
public class Vehicle {

	///<summary>The registration plate which identifies this vehicle, e.g. "OUTATIME"</summary>
	public string Registration { get; set; } = "";

	///<summary>The colour of this vehicle, e.g. "Turquoise"</summary>
	public string Color { get; set; } = "";

	///<summary>The year this vehicle was manufactured.</summary>
	///<example>2007</example>
	public int Year { get; set; }

	public VehicleModel Model { get; set; } = null!;

	///<summary>The code identifying which model of vehicle this is, e.g. "nissan-note"</summary>
	public string ModelCode { get; set; } = "";

}
