using Verum.Api.Configuracoes;
using Verum.CrossCutting;
using Verum.CrossCutting.Pipelines;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVerumApi(builder.Configuration);

builder.Services.AddVerumTratamentoErros();

var app = builder.Build();

app.UseVerumApi();

app.Run();

public partial class Program;
