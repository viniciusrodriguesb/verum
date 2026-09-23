using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Verum.CrossCutting.Pipelines;

public static class ApplicationPipeline
{
  public static WebApplication UseVerumApi(this WebApplication app)
  {
    app.UseExceptionHandler();

    app.UseStatusCodePages();

    if (!app.Environment.IsDevelopment())
      app.UseHsts();

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors(WebPipeline.CorsPolicy);

    app.UseAuthentication();

    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
      app.MapOpenApi();

    app.MapControllers();

    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

    app.MapHealthChecks("/health/ready");

    return app;
  }
}

