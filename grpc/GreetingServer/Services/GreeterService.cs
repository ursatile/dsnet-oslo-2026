using Grpc.Core;

namespace GreetingShared.Services;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase {
	public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context) {
		logger.LogInformation("The message is received from {Name}", request.Name);
		logger.LogInformation("The language code is received from {LanguageCode}", request.LanguageCode);

		var greeting = request.LanguageCode switch {
			"en-GB" => "Good Morning",
			"en-US" => "Howdy!",
			"en" => "Hello",
			"en-AU" => "G'day",
			"da" => "Hej",
			"nl-NL" => "Hallo",
			"tlh" => "nuqneH",
			_ => "Hello"
		};

		return Task.FromResult(new HelloReply {
			Message = $"{greeting} {request.Name}"
		});
	}
}

