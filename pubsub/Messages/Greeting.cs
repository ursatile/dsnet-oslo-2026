namespace Messages;

public class Greeting(string message, int number) {
	public string Message { get; set; } = message;
	public int Number { get; set; } = number;
	public override string ToString() => $"{Message} ({Number})";
}
