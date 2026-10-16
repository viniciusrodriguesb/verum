# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0.401-noble AS build
WORKDIR /src/backend
COPY src/backend/ ./
RUN --mount=type=cache,id=verum-nuget,target=/root/.nuget/packages dotnet restore Verum.Backend.slnx

FROM build AS publish-api
RUN --mount=type=cache,id=verum-nuget,target=/root/.nuget/packages \
    dotnet publish Apps/Verum.Api -c Release --no-restore -o /out/api /p:UseAppHost=false

FROM build AS publish-descoberta
RUN --mount=type=cache,id=verum-nuget,target=/root/.nuget/packages \
    dotnet publish Apps/Verum.Worker.Descoberta -c Release --no-restore -o /out/descoberta /p:UseAppHost=false

FROM build AS publish-radar
RUN --mount=type=cache,id=verum-nuget,target=/root/.nuget/packages \
    dotnet publish Apps/Verum.Worker.Radar -c Release --no-restore -o /out/radar /p:UseAppHost=false

FROM build AS browser-driver
RUN --mount=type=cache,id=verum-nuget,target=/root/.nuget/packages \
    version="$(sed -n 's/.*Include="Microsoft[.]Playwright" Version="\([^"]*\)".*/\1/p' Directory.Packages.props)" && \
    test -n "$version" && cp -a "/root/.nuget/packages/microsoft.playwright/$version/.playwright" /playwright-driver

FROM mcr.microsoft.com/dotnet/aspnet:10.0.13-noble AS runtime
ENV DOTNET_ENVIRONMENT=Production ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
USER root
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/* && mkdir -p /var/lib/verum/keys && chown -R app:app /var/lib/verum
USER app

FROM runtime AS api
COPY --from=publish-api --chown=app:app /out/api/ ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=4 \
  CMD bash -ec 'exec 3<>/dev/tcp/127.0.0.1/8080; printf "GET /health/live HTTP/1.0\r\nHost: ${HEALTHCHECK_HOST:-localhost}\r\n\r\n" >&3; head -n 1 <&3 | grep -q " 200 "'
ENTRYPOINT ["dotnet", "Verum.Api.dll"]

FROM runtime AS browser-runtime
USER root
COPY --from=browser-driver /playwright-driver/ /opt/playwright-driver/
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN /opt/playwright-driver/node/linux-x64/node /opt/playwright-driver/package/cli.js install --with-deps chromium && \
    chmod -R a+rX /ms-playwright && rm -rf /var/lib/apt/lists/* /opt/playwright-driver
USER app
HEALTHCHECK --interval=20s --timeout=5s --start-period=30s --retries=3 \
  CMD bash -ec 'for file in /proc/[0-9]*/comm; do read -r name < "$file" || continue; [ "$name" != dotnet ] || exit 0; done; exit 1'

FROM browser-runtime AS worker-descoberta
COPY --from=publish-descoberta --chown=app:app /out/descoberta/ ./
ENTRYPOINT ["dotnet", "Verum.Worker.Descoberta.dll"]

FROM browser-runtime AS worker-radar
COPY --from=publish-radar --chown=app:app /out/radar/ ./
ENTRYPOINT ["dotnet", "Verum.Worker.Radar.dll"]

FROM build AS migration-build
RUN --mount=type=cache,id=verum-nuget,target=/root/.nuget/packages dotnet tool restore --tool-manifest dotnet-tools.json
RUN --mount=type=cache,id=verum-nuget,target=/root/.nuget/packages \
    ConnectionStrings__PostgreSQL='Host=localhost;Database=build;Username=build;Password=unused' \
    bash -ec 'mkdir -p /out/migrations; for module in Contas Catalogo Ofertas Busca Radar Notificacoes Acesso Assinaturas; do \
      dotnet ef migrations bundle --project Modules/Verum.Modules.$module --context ${module}DbContext \
        --configuration Release --output /out/migrations/$module --force; \
    done'

FROM runtime AS migrations
COPY --from=migration-build --chown=app:app /out/migrations/ /app/
COPY --chmod=755 deploy/scripts/migrate.sh /app/migrate.sh
ENV DOTNET_BUNDLE_EXTRACT_BASE_DIR=/tmp/dotnet-bundles
ENTRYPOINT ["/app/migrate.sh"]
