#!/usr/bin/env bash
set -Eeuo pipefail
umask 077

root="${1:?Informe o diretório de implantação}"
service="${2:?Informe o serviço}"
image="${3:?Informe a imagem}"
registry_user="${4:?Informe o usuário do registry}"
migration_image="${5:-}"

[[ "$root" =~ ^/[a-zA-Z0-9/_-]+$ && "$root" != "/" ]] || { echo "Diretório inválido" >&2; exit 1; }
case "$service" in api|worker-descoberta|worker-radar|web|keycloak|migrations) ;; *) echo "Serviço inválido" >&2; exit 1 ;; esac
[[ "$image" =~ ^ghcr.io/[a-z0-9_.-]+/[a-z0-9_.-]+/$service@sha256:[a-f0-9]{64}$ ]] || { echo "Use imagem por digest" >&2; exit 1; }

repo="$(cd "$(dirname "$0")/../.." && pwd)"
export VERUM_CONFIG_DIR="$root/config"
mkdir -p "$root/locks" "$root/state"
exec 8>"$root/locks/$service.lock"
flock -x 8

export DOCKER_CONFIG
DOCKER_CONFIG="$(mktemp -d)"
trap 'rm -rf -- "$DOCKER_CONFIG"' EXIT
docker login ghcr.io --username "$registry_user" --password-stdin

compose() {
  local selected="$1"
  shift
  local env_args=()
  [[ ! -f "$VERUM_CONFIG_DIR/$selected.env" ]] || env_args=(--env-file "$VERUM_CONFIG_DIR/$selected.env")
  docker compose "${env_args[@]}" -p "verum-$selected" -f "$repo/deploy/$selected/compose.yaml" "$@"
}

migrate() {
  local migration_ref="$1"
  [[ "$migration_ref" =~ ^ghcr.io/[a-z0-9_.-]+/[a-z0-9_.-]+/migrations@sha256:[a-f0-9]{64}$ ]] || { echo "Imagem de migrations inválida" >&2; exit 1; }
  exec 9>"$root/locks/migrations.lock"
  flock -x 9
  if [[ ! -f "$root/state/migrations.current" ]] || [[ "$(cat "$root/state/migrations.current")" != "$migration_ref" ]]; then
    APP_IMAGE="$migration_ref" compose migrations pull migrations
    APP_IMAGE="$migration_ref" compose migrations run --rm --no-deps migrations
    printf '%s\n' "$migration_ref" > "$root/state/migrations.current"
  fi
  flock -u 9
}

if [[ "$service" == migrations ]]; then
  # O lock do serviço já cobre a execução; evita adquirir a mesma trava em outro descritor.
  APP_IMAGE="$image" compose migrations pull migrations
  APP_IMAGE="$image" compose migrations run --rm --no-deps migrations
else
  case "$service" in
    api|worker-descoberta|worker-radar)
      [[ -n "$migration_image" ]] || { echo "Informe o digest das migrations da mesma release" >&2; exit 1; }
      migrate "$migration_image"
      ;;
    keycloak)
      APP_IMAGE="$image" compose keycloak up -d --wait --no-recreate postgres
      ;;
  esac
  APP_IMAGE="$image" compose "$service" pull "$service"
  APP_IMAGE="$image" compose "$service" up -d --no-deps --wait --wait-timeout 180 "$service"
fi

if [[ -f "$root/state/$service.current" ]]; then cp "$root/state/$service.current" "$root/state/$service.previous"; fi
printf '%s\n' "$image" > "$root/state/$service.current"
echo "Implantação concluída: $service"
