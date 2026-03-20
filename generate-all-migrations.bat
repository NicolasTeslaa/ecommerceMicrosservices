@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM ========================================
REM Migration names
REM ========================================
set DELETE_PREVIOUS_MIGRATIONS=true
set CLEAN_BUILD_ARTIFACTS=true
set RETRY_FAILED_EF=true
set AUTH_MIGRATION=InitialAuth1
set CART_MIGRATION=InitialCart1
set CATALOG_WRITE_MIGRATION=InitialCatalogWrite1
set CATALOG_READ_MIGRATION=InitialCatalogRead1
set CUSTOMER_MIGRATION=InitialCustomer
set ORDER_WRITE_MIGRATION=InitialOrderWrite
set ORDER_READ_MIGRATION=InitialOrderRead

REM ========================================
REM Move to repository root
REM ========================================
cd /d "%~dp0"

set LOG_DIR=%CD%\migration-logs
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

call :check_tool dotnet || goto :fail
call :check_tool dotnet-ef || goto :fail

if /I "%DELETE_PREVIOUS_MIGRATIONS%"=="true" (
  echo.
  echo Deleting previous migration folders...
  if exist ".\ecommerce\services\AuthService\Auth.Infrastructure\Persistence\Migrations" rd /s /q ".\ecommerce\services\AuthService\Auth.Infrastructure\Persistence\Migrations"
  if exist ".\ecommerce\services\CartService\Cart.Infrastructure\Persistence\Migrations" rd /s /q ".\ecommerce\services\CartService\Cart.Infrastructure\Persistence\Migrations"
  if exist ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Persistence\Migrations" rd /s /q ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Persistence\Migrations"
  if exist ".\ecommerce\services\CustomerService\Customer.Infrastructure\Persistence\Migrations" rd /s /q ".\ecommerce\services\CustomerService\Customer.Infrastructure\Persistence\Migrations"
  if exist ".\ecommerce\services\OrderService\Order.Infrastructure\Persistence\Migrations" rd /s /q ".\ecommerce\services\OrderService\Order.Infrastructure\Persistence\Migrations"
)

echo Restoring solution...
dotnet restore ".\ecommerce\ecommerce-platform.slnx"
if errorlevel 1 goto :fail

call :run_migration "AuthService migration" ^
  ".\ecommerce\services\AuthService\Auth.Infrastructure\Auth.Infrastructure.csproj" ^
  ".\ecommerce\services\AuthService\Auth.API\Auth.API.csproj" ^
  "AuthDbContext" ^
  "%AUTH_MIGRATION%" ^
  "Persistence\Migrations" || goto :fail

call :run_migration "CartService migration" ^
  ".\ecommerce\services\CartService\Cart.Infrastructure\Cart.Infrastructure.csproj" ^
  ".\ecommerce\services\CartService\Cart.API\Cart.API.csproj" ^
  "CartDbContext" ^
  "%CART_MIGRATION%" ^
  "Persistence\Migrations" || goto :fail

call :run_migration "CatalogService write migration" ^
  ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  ".\ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj" ^
  "CatalogWriteDbContext" ^
  "%CATALOG_WRITE_MIGRATION%" ^
  "Persistence\Migrations\Write" || goto :fail

call :run_migration "CatalogService read migration" ^
  ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  ".\ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj" ^
  "CatalogReadDbContext" ^
  "%CATALOG_READ_MIGRATION%" ^
  "Persistence\Migrations\Read" || goto :fail

call :run_migration "CustomerService migration" ^
  ".\ecommerce\services\CustomerService\Customer.Infrastructure\Customer.Infrastructure.csproj" ^
  ".\ecommerce\services\CustomerService\Customer.API\Customer.API.csproj" ^
  "CustomerDbContext" ^
  "%CUSTOMER_MIGRATION%" ^
  "Persistence\Migrations" || goto :fail

call :run_migration "OrderService write migration" ^
  ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  ".\ecommerce\services\OrderService\Order.API.Write\Order.API.Write.csproj" ^
  "OrderWriteDbContext" ^
  "%ORDER_WRITE_MIGRATION%" ^
  "Persistence\Migrations\Write" || goto :fail

call :run_migration "OrderService read migration" ^
  ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  ".\ecommerce\services\OrderService\Order.API.Read\Order.API.Read.csproj" ^
  "OrderReadDbContext" ^
  "%ORDER_READ_MIGRATION%" ^
  "Persistence\Migrations\Read" || goto :fail

echo.
echo All migrations were generated successfully.

call :run_db_update "AuthService database" ^
  ".\ecommerce\services\AuthService\Auth.Infrastructure\Auth.Infrastructure.csproj" ^
  ".\ecommerce\services\AuthService\Auth.API\Auth.API.csproj" ^
  "AuthDbContext" || goto :fail

call :run_db_update "CartService database" ^
  ".\ecommerce\services\CartService\Cart.Infrastructure\Cart.Infrastructure.csproj" ^
  ".\ecommerce\services\CartService\Cart.API\Cart.API.csproj" ^
  "CartDbContext" || goto :fail

call :run_db_update "CatalogService write database" ^
  ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  ".\ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj" ^
  "CatalogWriteDbContext" || goto :fail

call :run_db_update "CatalogService read database" ^
  ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  ".\ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj" ^
  "CatalogReadDbContext" || goto :fail

call :run_db_update "CustomerService database" ^
  ".\ecommerce\services\CustomerService\Customer.Infrastructure\Customer.Infrastructure.csproj" ^
  ".\ecommerce\services\CustomerService\Customer.API\Customer.API.csproj" ^
  "CustomerDbContext" || goto :fail

call :run_db_update "OrderService write database" ^
  ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  ".\ecommerce\services\OrderService\Order.API.Write\Order.API.Write.csproj" ^
  "OrderWriteDbContext" || goto :fail

call :run_db_update "OrderService read database" ^
  ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  ".\ecommerce\services\OrderService\Order.API.Read\Order.API.Read.csproj" ^
  "OrderReadDbContext" || goto :fail

echo.
echo All migrations were generated and all databases were updated successfully.
echo Logs available at "%LOG_DIR%"
goto :end

:run_migration
set STEP_NAME=%~1
set PROJECT=%~2
set STARTUP=%~3
set DBCONTEXT=%~4
set MIGRATION_NAME=%~5
set OUTPUT_DIR=%~6

echo.
echo Creating %STEP_NAME%...
call :prepare_build "%PROJECT%" "%STARTUP%" || exit /b 1

call :run_ef "migrations add !MIGRATION_NAME! --context !DBCONTEXT! --project ""!PROJECT!"" --startup-project ""!STARTUP!"" --output-dir ""!OUTPUT_DIR!""" "!DBCONTEXT!_migration"
if errorlevel 1 exit /b 1
exit /b 0

:run_db_update
set STEP_NAME=%~1
set PROJECT=%~2
set STARTUP=%~3
set DBCONTEXT=%~4

echo.
echo Updating %STEP_NAME%...
call :prepare_build "%PROJECT%" "%STARTUP%" || exit /b 1

call :run_ef "database update --context !DBCONTEXT! --project ""!PROJECT!"" --startup-project ""!STARTUP!""" "!DBCONTEXT!_update"
if errorlevel 1 exit /b 1
exit /b 0

:prepare_build
set PROJECT=%~1
set STARTUP=%~2

if /I "%CLEAN_BUILD_ARTIFACTS%"=="true" (
  call :clean_project_artifacts "%PROJECT%"
  call :clean_project_artifacts "%STARTUP%"
  call :restore_project "%PROJECT%" || exit /b 1
  call :restore_project "%STARTUP%" || exit /b 1
)

call :build_project "%PROJECT%" || exit /b 1
call :build_project "%STARTUP%" || exit /b 1
exit /b 0

:clean_project_artifacts
set CSPROJ=%~1
for %%I in ("%CSPROJ%") do set PROJ_DIR=%%~dpI

if exist "!PROJ_DIR!bin" rd /s /q "!PROJ_DIR!bin"
if exist "!PROJ_DIR!obj" rd /s /q "!PROJ_DIR!obj"
exit /b 0

:build_project
set CSPROJ=%~1
for %%I in ("%CSPROJ%") do set LOG_NAME=%%~nI_build.log

echo Building "%CSPROJ%"...
dotnet build "%CSPROJ%" --no-restore > "%LOG_DIR%\!LOG_NAME!" 2>&1
if errorlevel 1 (
  echo Build failed for "%CSPROJ%".
  echo See "%LOG_DIR%\!LOG_NAME!" for details.
  type "%LOG_DIR%\!LOG_NAME!"
  exit /b 1
)
exit /b 0

:restore_project
set CSPROJ=%~1
for %%I in ("%CSPROJ%") do set LOG_NAME=%%~nI_restore.log

echo Restoring "%CSPROJ%"...
dotnet restore "%CSPROJ%" > "%LOG_DIR%\!LOG_NAME!" 2>&1
if errorlevel 1 (
  echo Restore failed for "%CSPROJ%".
  echo See "%LOG_DIR%\!LOG_NAME!" for details.
  type "%LOG_DIR%\!LOG_NAME!"
  exit /b 1
)
exit /b 0

:run_ef
set EF_ARGS=%~1
set LOG_BASENAME=%~2
set LOG_FILE=%LOG_DIR%\%LOG_BASENAME%.log

echo Running: dotnet ef %EF_ARGS%
dotnet ef %EF_ARGS% > "%LOG_FILE%" 2>&1
if not errorlevel 1 exit /b 0

if /I "%RETRY_FAILED_EF%"=="true" (
  echo dotnet ef failed. Retrying once...
  dotnet ef %EF_ARGS% >> "%LOG_FILE%" 2>&1
  if not errorlevel 1 exit /b 0
)

echo dotnet ef failed.
echo See "%LOG_FILE%" for details.
type "%LOG_FILE%"
exit /b 1

:check_tool
where %~1 >nul 2>&1
if errorlevel 1 (
  echo Required tool not found: %~1
  exit /b 1
)
exit /b 0

:fail
echo.
echo Migration generation failed. Review the logs in "%LOG_DIR%" and the command output above.
pause
exit /b 1

:end
echo.
pause
endlocal
