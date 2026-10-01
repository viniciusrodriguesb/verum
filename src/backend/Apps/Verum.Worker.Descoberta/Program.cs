using Verum.CrossCutting;
using Verum.Worker.Descoberta.Configuracoes;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddVerumWorkerDescoberta(builder.Configuration,
  bus => bus.AddConsumidoresDescoberta(builder.Configuration["Busca:FilaSolicitacoes"]));

await builder.Build().RunAsync();
