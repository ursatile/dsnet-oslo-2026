using Autobarn.AuditLog;
using EasyNetQ;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var rabbitmq = builder.Configuration.GetConnectionString("rabbitmq");
Console.WriteLine(rabbitmq);
builder.Services.AddEasyNetQ(rabbitmq);
builder.Services.AddHostedService<AuditLogService>();

var host = builder.Build();
host.Run();
