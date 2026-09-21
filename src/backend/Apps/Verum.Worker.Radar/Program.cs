using Microsoft.Extensions.Hosting;
using Verum.CrossCutting;
using Verum.Worker.Radar.Configuracoes;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddVerumWorkerRadar(builder.Configuration,
  bus => bus.AddConsumidoresRadar());
await builder.Build().RunAsync();
