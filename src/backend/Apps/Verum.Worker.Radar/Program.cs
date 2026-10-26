using Verum.CrossCutting;
using Verum.Worker.Radar.Agendamentos;
using Verum.Worker.Radar.Configuracoes;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddVerumWorkerRadar(builder.Configuration, bus => bus.AddConsumidoresRadar());

builder.Services.AddHostedService<AgendadorRadar>();

await builder.Build().RunAsync();
