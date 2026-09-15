using GreetingShared;
using Grpc.Net.Client;
Console.WriteLine("Hello, World!");

using var channel = GrpcChannel.ForAddress("http://localhost:5165");
var client = new Greeter.GreeterClient(channel);
Console.WriteLine("Press any key to send a request...");
Console.WriteLine("Press 1 for en-GB, 2 for en-US, 3 for en, 4 for da");
while (true) {
	var languageCode = Console.ReadKey().Key switch {
		ConsoleKey.D1 => "en-GB",
		ConsoleKey.D2 => "en-US",
		ConsoleKey.D3 => "en",
		ConsoleKey.D4 => "da",
		ConsoleKey.D5 => "nl-NL",
		ConsoleKey.D6 => "tlh",
		_ => "en"
	};
	var reply = await client.SayHelloAsync(new HelloRequest {
		FirstName = "NDC",
		LastName = "Oslo",
		LanguageCode = languageCode
	});
	Console.WriteLine(reply.Message);
}
