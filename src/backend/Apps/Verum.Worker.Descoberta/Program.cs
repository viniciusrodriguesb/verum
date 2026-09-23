using Verum.CrossCutting;
using Verum.Worker.Descoberta.Configuracoes;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddVerumWorkerDescoberta(builder.Configuration, bus => bus.AddConsumidoresDescoberta());

await builder.Build().RunAsync();
