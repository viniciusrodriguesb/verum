#!/usr/bin/env bash
set -Eeuo pipefail
repo="$(cd "$(dirname "$0")/../.." && pwd)"
temp="$(mktemp -d /tmp/verumdeployXXXXXXXX)"
trap 'rm -rf -- "$temp"' EXIT
mkdir -p "$temp/bin" "$temp/config"
export DEPLOY_TEST_LOG="$temp/commands"
cat > "$temp/bin/docker" <<'MOCK'
#!/usr/bin/env bash
set -eu
if [[ "$1" == login ]]; then cat >/dev/null; exit 0; fi
printf '%s\n' "$*" >> "$DEPLOY_TEST_LOG"
if [[ " $* " == *" run "* && "${DEPLOY_TEST_FAIL:-0}" == 1 ]]; then exit 1; fi
MOCK
chmod +x "$temp/bin/docker"
export PATH="$temp/bin:$PATH"
digest="$(printf '%064d' 1)"
migrations="ghcr.io/test/verum/migrations@sha256:$digest"
deploy() {
  printf 'temporary-token' | bash "$repo/deploy/scripts/deploy-service.sh" \
    "$temp" "$1" "ghcr.io/test/verum/$1@sha256:$digest" test "${2:-}"
}
deploy api "$migrations"
deploy worker-descoberta "$migrations"
[[ "$(grep -c 'run --rm --no-deps migrations' "$DEPLOY_TEST_LOG")" == 1 ]]
grep -q 'up -d --no-deps --wait --wait-timeout 180 api$' "$DEPLOY_TEST_LOG"
grep -q 'up -d --no-deps --wait --wait-timeout 180 worker-descoberta$' "$DEPLOY_TEST_LOG"
deploy web
if grep -Eq ' down| prune| up .* (postgres|keycloak)$' "$DEPLOY_TEST_LOG"; then exit 1; fi
[[ -f "$temp/state/api.current" && -f "$temp/state/web.current" ]]
rm "$temp/state/migrations.current"
export DEPLOY_TEST_FAIL=1
if deploy worker-radar "$migrations"; then
  echo 'Uma migration que falha deve bloquear o deploy.' >&2
  exit 1
fi
[[ ! -f "$temp/state/worker-radar.current" ]]
if grep -q 'up .* worker-radar$' "$DEPLOY_TEST_LOG"; then exit 1; fi
echo 'Deploy isolado, migrations idempotentes e bloqueio por falha: OK.'
