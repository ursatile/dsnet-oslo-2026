using System.Media;
using EasyNetQ;
using Messages;
using Microsoft.Extensions.DependencyInjection;

var player = new SoundPlayer("car_horn.wav");
const string AMQP = "amqps://oyhhpspg:TkkZNbWNOG5B2TjctWXr0r-W00V4upzP@metallic-yellow-chicken.rmq7.cloudamqp.com/oyhhpspg";
var serviceCollection = new ServiceCollection();
serviceCollection.AddEasyNetQ(AMQP).UseSystemTextJson();
using var provider = serviceCollection.BuildServiceProvider();
var bus = provider.GetRequiredService<IBus>();

Console.WriteLine("Subscribing to messages...");
var subscription = await bus.PubSub.SubscribeAsync<Greeting>("dylanbeattie-2", message => {
	if (message.Number % 5 == 0) {
		throw new Exception("Oops, something went wrong!");
	}
	Console.WriteLine($"Received: {message}");
	player.Play();
});

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
await subscription.DisposeAsync();
