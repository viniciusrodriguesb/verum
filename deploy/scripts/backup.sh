#!/usr/bin/env bash
set -Eeuo pipefail
umask 077

repo="$(cd "$(dirname "$0")/../.." && pwd)"
config="${VERUM_CONFIG_DIR:-/opt/verum/config}"
destination="${1:?Informe o diretório privado de backups}"

export VERUM_CONFIG_DIR="$config"
export APP_IMAGE="${APP_IMAGE:-ghcr.io/validation/unused@sha256:$(printf '%064d' 0)}"

[[ -d "$config" ]] || { echo "Diretório de configuração inexistente" >&2; exit 1; }
mkdir -p -- "$destination"
destination="$(cd "$destination" && pwd)"
temporary="$(mktemp -d "$destination/.verum-backup-XXXXXX")"
trap 'rm -rf -- "$temporary"' EXIT

docker compose --env-file "$config/infra.env" -p verum-infra -f "$repo/deploy/infra/compose.yaml" \
  exec -T postgres sh -ec 'exec pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom' > "$temporary/verum.dump"

docker compose --env-file "$config/keycloak.env" -p verum-keycloak -f "$repo/deploy/keycloak/compose.yaml" \
  exec -T postgres sh -ec 'exec pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom' > "$temporary/keycloak.dump"

[[ -s "$temporary/verum.dump" && -s "$temporary/keycloak.dump" ]]
(cd "$temporary" && sha256sum verum.dump keycloak.dump > SHA256SUMS)

final="$destination/verum-$(date -u +%Y%m%dT%H%M%SZ)-${temporary##*-}"
mv -- "$temporary" "$final"
trap - EXIT
printf 'Backup concluído: %s\n' "$final"
