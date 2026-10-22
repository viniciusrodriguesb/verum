using Verum.CrossCutting;
using Verum.Worker.Radar.Configuracoes;
using Verum.Worker.Radar.Agendamentos;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddVerumWorkerRadar(builder.Configuration, bus => bus.AddConsumidoresRadar());

builder.Services.AddHostedService<AgendadorRadar>();

await builder.Build().RunAsync();
