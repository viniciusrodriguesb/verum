# syntax=docker/dockerfile:1
FROM node:24.19.0-bookworm-slim AS realm
WORKDIR /realm
COPY src/keycloak/realms/verum-realm.json ./
RUN node -e 'const fs=require("fs");const r=JSON.parse(fs.readFileSync("verum-realm.json"));r.sslRequired="all";r.smtpServer={host:"${VERUM_SMTP_HOST}",port:"${VERUM_SMTP_PORT}",from:"${VERUM_SMTP_FROM}",fromDisplayName:"Verum",auth:"true",user:"${VERUM_SMTP_USER}",password:"${VERUM_SMTP_PASSWORD}",ssl:"false",starttls:"true"};fs.writeFileSync("verum-realm.json",JSON.stringify(r));'

FROM quay.io/keycloak/keycloak:26.8.0 AS build
ENV KC_DB=postgres KC_HEALTH_ENABLED=true KC_METRICS_ENABLED=true
RUN /opt/keycloak/bin/kc.sh build

FROM quay.io/keycloak/keycloak:26.8.0
COPY --from=build /opt/keycloak/ /opt/keycloak/
COPY --from=realm --chown=keycloak:keycloak /realm/verum-realm.json /opt/keycloak/data/import/verum-realm.json
COPY --chown=keycloak:keycloak src/keycloak/themes/ /opt/keycloak/themes/
ENV KC_DB=postgres KC_HEALTH_ENABLED=true KC_METRICS_ENABLED=true
USER 1000
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=90s --retries=10 \
  CMD bash -ec '{ printf "HEAD /health/ready HTTP/1.0\r\n\r\n" >&0; grep -q "HTTP/1.0 200"; } 0<>/dev/tcp/localhost/9000'
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start", "--optimized", "--import-realm"]
