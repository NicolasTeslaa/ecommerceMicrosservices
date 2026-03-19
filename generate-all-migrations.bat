@echo off
setlocal

REM ========================================
REM Migration names
REM ========================================
set DELETE_PREVIOUS_MIGRATIONS=true
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

echo.
echo Creating AuthService migration...
dotnet ef migrations add %AUTH_MIGRATION% ^
  --context AuthDbContext ^
  --project ".\ecommerce\services\AuthService\Auth.Infrastructure\Auth.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\AuthService\Auth.API\Auth.API.csproj" ^
  --output-dir "Persistence\Migrations"
if errorlevel 1 goto :fail

echo.
echo Creating CartService migration...
dotnet ef migrations add %CART_MIGRATION% ^
  --context CartDbContext ^
  --project ".\ecommerce\services\CartService\Cart.Infrastructure\Cart.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CartService\Cart.API\Cart.API.csproj" ^
  --output-dir "Persistence\Migrations"
if errorlevel 1 goto :fail

echo.
echo Creating CatalogService write migration...
dotnet ef migrations add %CATALOG_WRITE_MIGRATION% ^
  --context CatalogWriteDbContext ^
  --project ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj" ^
  --output-dir "Persistence\Migrations\Write"
if errorlevel 1 goto :fail

echo.
echo Creating CatalogService read migration...
dotnet ef migrations add %CATALOG_READ_MIGRATION% ^
  --context CatalogReadDbContext ^
  --project ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj" ^
  --output-dir "Persistence\Migrations\Read"
if errorlevel 1 goto :fail

echo.
echo Creating CustomerService migration...
dotnet ef migrations add %CUSTOMER_MIGRATION% ^
  --context CustomerDbContext ^
  --project ".\ecommerce\services\CustomerService\Customer.Infrastructure\Customer.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CustomerService\Customer.API\Customer.API.csproj" ^
  --output-dir "Persistence\Migrations"
if errorlevel 1 goto :fail

echo.
echo Creating OrderService write migration...
dotnet ef migrations add %ORDER_WRITE_MIGRATION% ^
  --context OrderWriteDbContext ^
  --project ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\OrderService\Order.API.Write\Order.API.Write.csproj" ^
  --output-dir "Persistence\Migrations\Write"
if errorlevel 1 goto :fail

echo.
echo Creating OrderService read migration...
dotnet ef migrations add %ORDER_READ_MIGRATION% ^
  --context OrderReadDbContext ^
  --project ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\OrderService\Order.API.Read\Order.API.Read.csproj" ^
  --output-dir "Persistence\Migrations\Read"
if errorlevel 1 goto :fail

echo.
echo All migrations were generated successfully.
echo.
echo Updating AuthService database...
dotnet ef database update ^
  --context AuthDbContext ^
  --project ".\ecommerce\services\AuthService\Auth.Infrastructure\Auth.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\AuthService\Auth.API\Auth.API.csproj"
if errorlevel 1 goto :fail

echo.
echo Updating CartService database...
dotnet ef database update ^
  --context CartDbContext ^
  --project ".\ecommerce\services\CartService\Cart.Infrastructure\Cart.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CartService\Cart.API\Cart.API.csproj"
if errorlevel 1 goto :fail

echo.
echo Updating CatalogService write database...
dotnet ef database update ^
  --context CatalogWriteDbContext ^
  --project ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj"
if errorlevel 1 goto :fail

echo.
echo Updating CatalogService read database...
dotnet ef database update ^
  --context CatalogReadDbContext ^
  --project ".\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj"
if errorlevel 1 goto :fail

echo.
echo Updating CustomerService database...
dotnet ef database update ^
  --context CustomerDbContext ^
  --project ".\ecommerce\services\CustomerService\Customer.Infrastructure\Customer.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\CustomerService\Customer.API\Customer.API.csproj"
if errorlevel 1 goto :fail

echo.
echo Updating OrderService write database...
dotnet ef database update ^
  --context OrderWriteDbContext ^
  --project ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\OrderService\Order.API.Write\Order.API.Write.csproj"
if errorlevel 1 goto :fail

echo.
echo Updating OrderService read database...
dotnet ef database update ^
  --context OrderReadDbContext ^
  --project ".\ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj" ^
  --startup-project ".\ecommerce\services\OrderService\Order.API.Read\Order.API.Read.csproj"
if errorlevel 1 goto :fail

echo.
echo All migrations were generated and all databases were updated successfully.
goto :end

:fail
echo.
echo Migration generation failed. Review the command output above.
pause
exit /b 1

:end
echo.
pause
endlocal
