#!/usr/bin/env bash
set -Eeuo pipefail
repo="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$repo"
temp="$(mktemp -d)"
trap 'rm -rf -- "$temp"' EXIT
for example in deploy/config/*.env.example; do
  cp "$example" "$temp/$(basename "${example%.example}")"
done
cp deploy/config/web.configuracao.json.example "$temp/web.configuracao.json"
export VERUM_CONFIG_DIR="$temp"
export APP_IMAGE
APP_IMAGE="ghcr.io/validation/verum/api@sha256:$(printf '%064d' 0)"
for service in api worker-descoberta worker-radar web keycloak migrations infra edge; do
  env_args=()
  [[ ! -f "$temp/$service.env" ]] || env_args=(--env-file "$temp/$service.env")
  docker compose "${env_args[@]}" -p "verum-validate-$service" -f "deploy/$service/compose.yaml" config --quiet
done
for script in deploy/scripts/*.sh; do bash -n "$script"; done
echo "Manifests e scripts válidos."
