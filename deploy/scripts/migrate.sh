#!/usr/bin/env bash
set -Eeuo pipefail
: "${ConnectionStrings__PostgreSQL:?Configure a conexão PostgreSQL de destino}"
for module in Contas Catalogo Ofertas Busca Radar Notificacoes Acesso Assinaturas; do
  echo "Aplicando migrations: $module"
  "/app/$module"
done
