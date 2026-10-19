# Loyalty Benefit API

SaaS multi-tenant para programas de lealtad. Permite a empresas (inicialmente hoteles) registrar clientes, acumular puntos por consumo y canjearlos por beneficios.

## 🛠 Stack Técnico
- **Backend:** .NET 10 (C#) con Minimal APIs
- **Base de Datos:** SQL Server 2022 Express + EF Core (Platform DB + Tenant DBs)
- **Frontend:** Flutter (en desarrollo)
- **Documentación API:** Swagger / OpenAPI

## 🚀 Cómo correr el proyecto en local

### Requisitos
- .NET 10 SDK
- SQL Server 2022 Express (`SQLEXPRESS2022`)
- Git

### Pasos
1. Clonar el repositorio.
2. Crear el archivo `src/Loyaltybenefit.Api/appsettings.Development.json` (ver `CONTINUIDAD.md` sección 4 para el contenido exacto).
3. Aplicar migraciones:
   ```powershell
   dotnet ef database update --project src\Loyaltybenefit.Infrastructure\Loyaltybenefit.Infrastructure.csproj --startup-project src\Loyaltybenefit.Api\Loyaltybenefit.Api.csproj --context PlatformDbContext
