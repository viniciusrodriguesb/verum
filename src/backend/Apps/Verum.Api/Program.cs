using Verum.CrossCutting;
using Verum.CrossCutting.Pipelines;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVerumApi(builder.Configuration);

var app = builder.Build();

app.UseVerumApi();

app.Run();

public partial class Program;
