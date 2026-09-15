using Autobarn.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

//var rabbitMqUsername = builder.AddParameter("username", "admin", secret: true);
//var rabbitMqPassword = builder.AddParameter("password", "secret", secret: true);

//var rabbitmq = builder.AddRabbitMQ(
//		"rabbitmq",
//		userName: rabbitMqUsername,
//		password: rabbitMqPassword)
//	.WithContainerName("autobarn-rabbitmq")
//	.WithLifetime(ContainerLifetime.Persistent)
//	.WithManagementPlugin();

// var pricingServer = builder
// 	// The "http" profile keeps this off https://localhost:5003, which belongs to the Node.js pricing server.
// 	.AddProject<Projects.Autobarn_PricingServer>("autobarn-pricing-server", launchProfileName: "http")
// 	.WithHttpEndpoint(name: ConfigKeys.GrpcPricingServerUrl);

// Node.js implementation of the same Pricer contract (Autobarn.PricingServer/Protos/price.proto).
// Aspire runs `npm install` first, injects the OTEL_* variables so traces, metrics and logs reach the
// dashboard, and hands the Node process the ASP.NET Core dev certificate as PEM files for TLS.
#pragma warning disable ASPIRECERTIFICATES001 // Dev certificate APIs are experimental
var nodePricingServer = builder
	.AddNodeApp("autobarn-pricing-server-node", "../Autobarn.PricingServer.Node", "server.js")
	.WithHttpsEndpoint(port: 5003, env: "PORT", isProxied: false)
	.WithHttpsDeveloperCertificate()
	.WithHttpsCertificateConfiguration(ctx => {
		ctx.EnvironmentVariables["TLS_CERT_PATH"] = ctx.CertificatePath;
		ctx.EnvironmentVariables["TLS_KEY_PATH"] = ctx.KeyPath;
		return Task.CompletedTask;
	});
#pragma warning restore ASPIRECERTIFICATES001

builder.AddProject<Projects.Autobarn_AuditLog>("auditlog");
	//.WithReference(rabbitmq)
	//.WaitFor(rabbitmq);

var website = builder.AddProject<Projects.Autobarn_Website>("autobarn-website")
	//.WaitFor(rabbitmq)
	//.WithReference(rabbitmq)
	.WithHttpEndpoint(name: ConfigKeys.AutobarnWebsiteUrl);

//builder.AddProject<Projects.Autobarn_Notifier>("notifier");
	//.WithReference(rabbitmq)
	//.WaitFor(rabbitmq)
	//.WithReference(website)
	//.WithEnvironment(ConfigKeys.AutobarnWebsiteUrl, website.GetEndpoint(ConfigKeys.AutobarnWebsiteUrl))
	//.WaitFor(website);

//builder.AddProject<Projects.Autobarn_PricingClient>("pricing-client")
//	//.WithReference(rabbitmq)
//	//.WaitFor(rabbitmq)
//	.WithReference(nodePricingServer)
//	.WaitFor(nodePricingServer)
//	.WithEnvironment(
//		ConfigKeys.GrpcPricingServerUrl,
//		nodePricingServer.GetEndpoint("https"));
//	// pricingServer.GetEndpoint(ConfigKeys.GrpcPricingServerUrl));

builder.Build().Run();
