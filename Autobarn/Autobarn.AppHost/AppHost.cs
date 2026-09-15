using Autobarn.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitMqUsername = builder.AddParameter("username", "admin", secret: true);
var rabbitMqPassword = builder.AddParameter("password", "secret", secret: true);

var rabbitmq = builder.AddRabbitMQ(
		"rabbitmq",
		userName: rabbitMqUsername,
		password: rabbitMqPassword)
	.WithContainerName("autobarn-rabbitmq")
	.WithLifetime(ContainerLifetime.Persistent)
	.WithManagementPlugin();

var pricingServer = builder
	.AddProject<Projects.Autobarn_PricingServer>("autobarn-pricing-server")
	.WithHttpEndpoint(name: ConfigKeys.GrpcPricingServerUrl);

builder.AddProject<Projects.Autobarn_AuditLog>("auditlog")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq);

builder.AddProject<Projects.Autobarn_PricingClient>("pricing-client")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq)
	.WithReference(pricingServer)
	.WaitFor(pricingServer)
	.WithEnvironment(ConfigKeys.GrpcPricingServerUrl, pricingServer.GetEndpoint(ConfigKeys.GrpcPricingServerUrl));

var website = builder.AddProject<Projects.Autobarn_Website>("autobarn-website")
	.WaitFor(rabbitmq)
	.WithReference(rabbitmq);

builder.Build().Run();
