using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Verum.CrossCutting.Pipelines;

public static class AutenticacaoPipeline
{
  public static IServiceCollection AddVerumAutenticacao(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddAuthorization();

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
      options.MapInboundClaims = false;

      options.RequireHttpsMetadata = configuration.GetValue("Authentication:RequireHttpsMetadata", true);

      options.Authority = configuration["Authentication:Authority"];

      options.Audience = configuration["Authentication:Audience"];

      options.TokenValidationParameters.ValidateIssuer = true;

      options.TokenValidationParameters.ValidateAudience = true;

      options.TokenValidationParameters.ValidateLifetime = true;

      options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);

      options.TokenValidationParameters.NameClaimType = "preferred_username";
    });

    services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
      .Validate(o => Uri.TryCreate(o.Authority, UriKind.Absolute, out var authority)
        && (authority.Scheme == "https" || !o.RequireHttpsMetadata && authority.Scheme == "http"),
        "Configure Authentication:Authority com a URL do realm Keycloak.")
      .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Configure Authentication:Audience.")
      .ValidateOnStart();

    return services;
  }
}

