#!/usr/bin/env bash
set -Eeuo pipefail
repo="$(cd "$(dirname "$0")/../.." && pwd)"
temporary="$(mktemp -d)"
trap 'rm -rf -- "$temporary"' EXIT
mkdir -p "$temporary/bin" "$temporary/config" "$temporary/ok" "$temporary/fail"

cat > "$temporary/bin/docker" <<'MOCK'
#!/usr/bin/env bash
set -Eeuo pipefail
if [[ "$*" == *verum-keycloak* && "${FAIL_KEYCLOAK:-0}" == 1 ]]; then exit 1; fi
printf 'dump de teste\n'
MOCK
chmod +x "$temporary/bin/docker"
export PATH="$temporary/bin:$PATH"
export VERUM_CONFIG_DIR="$temporary/config"

bash "$repo/deploy/scripts/backup.sh" "$temporary/ok"
mapfile -t completed < <(find "$temporary/ok" -mindepth 1 -maxdepth 1 -type d)
[[ "${#completed[@]}" == 1 ]]
(cd "${completed[0]}" && sha256sum -c SHA256SUMS)
[[ "$(stat -c %a "${completed[0]}")" == 700 ]]
[[ "$(stat -c %a "${completed[0]}/verum.dump")" == 600 ]]

if FAIL_KEYCLOAK=1 bash "$repo/deploy/scripts/backup.sh" "$temporary/fail"; then
  echo "O backup parcial deveria falhar" >&2
  exit 1
fi
[[ -z "$(find "$temporary/fail" -mindepth 1 -print -quit)" ]]
echo "Backup: checksums, permissões e descarte de cópia parcial validados."
