namespace GreetingShared;
public partial class HelloRequest {
	public string Name {
		get => $"{FirstName} {LastName}";
		set => throw new NotImplementedException("Name is a computed property. Set FirstName and LastName instead.");
	}
}
