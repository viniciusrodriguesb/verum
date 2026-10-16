#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
root="${1:-/opt/verum}"
[[ "$root" =~ ^/[a-zA-Z0-9/_-]+$ && "$root" != "/" ]] || { echo "Diretório inválido" >&2; exit 1; }
command -v docker >/dev/null
docker compose version >/dev/null
mkdir -p "$root/config" "$root/releases" "$root/state" "$root/locks"
if ! docker network inspect verum >/dev/null 2>&1; then
  docker network create --subnet 172.30.0.0/24 verum >/dev/null
fi
actual="$(docker network inspect verum --format '{{(index .IPAM.Config 0).Subnet}}')"
[[ "$actual" == "172.30.0.0/24" ]] || { echo "A rede verum existente possui outro subnet. Revise a confiança do proxy antes de continuar." >&2; exit 1; }
echo "Diretórios e rede preparados. Preencha os arquivos de configuração antes de implantar."
