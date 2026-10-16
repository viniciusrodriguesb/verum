# syntax=docker/dockerfile:1
FROM node:24.19.0-bookworm-slim AS build
WORKDIR /app
COPY src/frontend/web/package*.json ./
RUN --mount=type=cache,id=verum-npm,target=/root/.npm npm ci --no-audit --no-fund
COPY src/frontend/web/ ./
RUN npm run build:production

FROM nginxinc/nginx-unprivileged:stable-alpine@sha256:782bb89768cc87b7f95c18b95baa1af250e5d451f26794f59363540161147c01
COPY --from=build --chown=101:101 /app/dist/verum-web/browser/ /usr/share/nginx/html/
COPY src/frontend/web/deploy/default.conf /etc/nginx/conf.d/default.conf
USER 101
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=10s --retries=3 \
  CMD wget -q -O /dev/null http://127.0.0.1:8080/health
