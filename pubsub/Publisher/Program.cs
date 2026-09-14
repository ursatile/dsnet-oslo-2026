using EasyNetQ;
using Messages;
using Microsoft.Extensions.DependencyInjection;

const string AMQP = "amqps://oyhhpspg:TkkZNbWNOG5B2TjctWXr0r-W00V4upzP@metallic-yellow-chicken.rmq7.cloudamqp.com/oyhhpspg";

var serviceCollection = new ServiceCollection();
serviceCollection.AddEasyNetQ(AMQP).UseSystemTextJson();

using var provider = serviceCollection.BuildServiceProvider();
var bus = provider.GetRequiredService<IBus>();
var number = 0;
Console.WriteLine("Press any key to publish a message...");
while (true) {
	Console.ReadKey();
	var message = new Greeting($"Hello from {Environment.MachineName}", number++);
	await bus.PubSub.PublishAsync(message);
	Console.WriteLine($"Published: {message}");
}
