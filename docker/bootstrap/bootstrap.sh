#!/bin/sh
set -eu

ROOT_DIR="/src"
MYSQL_HOST="${MYSQL_HOST:-mysql}"
MYSQL_PORT="${MYSQL_PORT:-3306}"
MYSQL_USER="${MYSQL_USER:-root}"
MYSQL_ROOT_PASSWORD="${MYSQL_ROOT_PASSWORD:-12345678}"
FORCE_CATALOG_SEED="${FORCE_CATALOG_SEED:-false}"

export PATH="$PATH:/root/.dotnet/tools"

mysql_exec() {
  mysql \
    --protocol=TCP \
    -h "$MYSQL_HOST" \
    -P "$MYSQL_PORT" \
    -u "$MYSQL_USER" \
    -p"$MYSQL_ROOT_PASSWORD" \
    "$@"
}

wait_for_mysql() {
  echo "Waiting for MySQL at ${MYSQL_HOST}:${MYSQL_PORT}..."

  until mysql_exec -e "SELECT 1" >/dev/null 2>&1; do
    sleep 3
  done

  echo "MySQL is available."
}

create_databases() {
  echo "Ensuring application databases exist..."

  mysql_exec -e "
    CREATE DATABASE IF NOT EXISTS \`ecommerce-platform-auth\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-plataform-cart\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-plataform-catalog-write\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-plataform-catalog-read\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-platform-customer\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-platform-order-write\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-platform-order-read\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-platform-payment\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-platform-inventory\`;
    CREATE DATABASE IF NOT EXISTS \`ecommerce-platform-nota-fiscal\`;
  "
}

run_migration() {
  name="$1"
  project="$2"
  startup="$3"
  context="$4"

  echo "Applying migrations for ${name}..."
  dotnet ef database update \
    --project "$project" \
    --startup-project "$startup" \
    --context "$context"
}

run_migrations() {
  echo "Restoring solution..."
  dotnet restore "${ROOT_DIR}/ecommerce/ecommerce-platform.slnx"

  run_migration \
    "AuthService" \
    "${ROOT_DIR}/ecommerce/services/AuthService/Auth.Infrastructure/Auth.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/AuthService/Auth.API/Auth.API.csproj" \
    "AuthDbContext"

  run_migration \
    "CartService" \
    "${ROOT_DIR}/ecommerce/services/CartService/Cart.Infrastructure/Cart.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/CartService/Cart.API/Cart.API.csproj" \
    "CartDbContext"

  run_migration \
    "CatalogService Write" \
    "${ROOT_DIR}/ecommerce/services/CatalogService/Catalog.Infrastructure/Catalog.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/CatalogService/Catalog.API.Write/Catalog.API.Write.csproj" \
    "CatalogWriteDbContext"

  run_migration \
    "CatalogService Read" \
    "${ROOT_DIR}/ecommerce/services/CatalogService/Catalog.Infrastructure/Catalog.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/CatalogService/Catalog.API.Read/Catalog.API.Read.csproj" \
    "CatalogReadDbContext"

  run_migration \
    "CustomerService" \
    "${ROOT_DIR}/ecommerce/services/CustomerService/Customer.Infrastructure/Customer.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/CustomerService/Customer.API/Customer.API.csproj" \
    "CustomerDbContext"

  run_migration \
    "OrderService Write" \
    "${ROOT_DIR}/ecommerce/services/OrderService/Order.Infrastructure/Order.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/OrderService/Order.API.Write/Order.API.Write.csproj" \
    "OrderWriteDbContext"

  run_migration \
    "OrderService Read" \
    "${ROOT_DIR}/ecommerce/services/OrderService/Order.Infrastructure/Order.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/OrderService/Order.API.Read/Order.API.Read.csproj" \
    "OrderReadDbContext"

  run_migration \
    "PaymentService" \
    "${ROOT_DIR}/ecommerce/services/PaymentService/Payment.Infrastructure/Payment.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/PaymentService/Payment.API/Payment.API.csproj" \
    "PaymentDbContext"

  run_migration \
    "InventoryService" \
    "${ROOT_DIR}/ecommerce/services/InventoryService/Inventory.Infrastructure/Inventory.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/InventoryService/Inventory.API/Inventory.API.csproj" \
    "InventoryDbContext"

  run_migration \
    "NotaFiscalService" \
    "${ROOT_DIR}/ecommerce/services/NotaFiscalService/NotaFiscal.Infrastructure/NotaFiscal.Infrastructure.csproj" \
    "${ROOT_DIR}/ecommerce/services/NotaFiscalService/NotaFiscal.API/NotaFiscal.API.csproj" \
    "NotaFiscalDbContext"
}

should_seed_catalog() {
  if [ "$FORCE_CATALOG_SEED" = "true" ]; then
    return 0
  fi

  current_count="$(mysql_exec -Nse 'SELECT COUNT(*) FROM `ecommerce-plataform-catalog-write`.products;' 2>/dev/null || echo 0)"

  [ "$current_count" = "0" ]
}

seed_catalog() {
  echo "Loading catalog seed from seed-catalog-50k.sql..."
  mysql_exec < "${ROOT_DIR}/seed-catalog-50k.sql"
}

main() {
  cd "$ROOT_DIR"

  wait_for_mysql
  create_databases
  run_migrations

  if should_seed_catalog; then
    seed_catalog
  else
    echo "Catalog seed skipped because products already exist. Set FORCE_CATALOG_SEED=true to reload it."
  fi

  echo "Bootstrap finished successfully."
}

main "$@"
