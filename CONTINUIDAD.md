\# 🔄 DOCUMENTO DE CONTINUIDAD — LOYALTY BENEFIT



\*\*Propósito:\*\* Permitir retomar el proyecto en 5 minutos si algo sale mal (cambio de equipo, pérdida de disco, ausencia prolongada, o simplemente "no recuerdo dónde quedó").



\*\*Última actualización:\*\* 16/Oct/2026, sesión 1 de rescate y organización.

\*\*Estado del proyecto:\*\* Backend operativo. Frontend pendiente.



\---



\## 📌 1. RESUMEN EJECUTIVO (30 segundos de lectura)



\- \*\*Producto:\*\* Loyalty Benefit — SaaS de programa de lealtad para hoteles (suma puntos por consumo, canje por productos/servicios).

\- \*\*Stack:\*\* .NET 10 + SQL Server 2022 Express + Flutter (pendiente).

\- \*\*Arquitectura:\*\* Multi-tenant con BD independiente por tenant (Platform DB + Tenant DB).

\- \*\*Primer cliente objetivo:\*\* Concept Hotel (Zamora, Michoacán, 38 habitaciones).

\- \*\*Avance real:\*\* \~35% (backend 85% funcional, frontend 0%).

\- \*\*Estado hoy:\*\* Backend compila, corre, Swagger operativo, BD creada con migraciones.



\---



\## 🗂️ 2. ESTRUCTURA DE CARPETAS (dónde vive cada cosa)



```

C:\\Aplica\\

├── Loyaltybenefit\_API\\              ← 🎯 REPO PRINCIPAL — Backend .NET

│   ├── .git\\                        ← Historial de Git

│   ├── .gitignore                   ← Excluye bin/, obj/, appsettings.Development.json

│   ├── Loyaltybenefit\_API.slnx      ← Solución .NET

│   ├── Dockerfile.api               ← Docker para API (futuro)

│   ├── Dockerfile.worker            ← Docker para Worker (futuro)

│   ├── docker-compose.yml           ← Compose para desarrollo

│   ├── .env.example                 ← Plantilla de variables de entorno

│   ├── README.md                    ← (PENDIENTE crear)

│   ├── CONTINUIDAD.md               ← Este archivo

│   ├── BITACORA.md                  ← Bitácora de sesiones

│   ├── src\\

│   │   ├── Loyaltybenefit.Api\\           ← API (endpoints, Program.cs)

│   │   │   ├── appsettings.json          ← Configuración base

│   │   │   ├── appsettings.Development.json  ← ⚠️ NO EN GIT — configuración local

│   │   │   └── Program.cs

│   │   ├── Loyaltybenefit.Application\\   ← Lógica de negocio (Commands, Queries)

│   │   ├── Loyaltybenefit.Domain\\        ← Entidades (Tenant, LoyaltyAccount, etc.)

│   │   ├── Loyaltybenefit.Infrastructure\\← EF Core, migraciones, repositorios

│   │   └── Loyaltybenefit.Worker\\        ← Worker para jobs en background

│   └── tests\\

│       └── Loyaltybenefit.Tests\\         ← Tests unitarios (casi vacío aún)

│

├── Loyaltybenefit\_App\\              ← 🎯 REPO PRINCIPAL — Frontend Flutter (VACÍO)

│   ├── .git\\

│   ├── .gitignore

│   └── README.md                    ← (PENDIENTE — proyecto Flutter aún no creado)

│

├── Loyaltybenefit\_Legacy\\           ← 📚 REFERENCIA (solo lectura)

│   ├── api-legacy\\                  ← EIE original (127 archivos .cs) — congit tag

│   ├── app-legacy-vacio\\            ← Vacío (nunca hubo frontend)

│   ├── database\\

│   ├── docs\\

│   ├── scripts\\

│   └── tools\\

│

└── \_backup-20261016-2001\\           ← Backup del día 16/Oct (borrar en 1 semana)

```



\---



\## 🔗 3. REPOSITORIOS DE GITHUB



| Repo | URL | Estado |

|---|---|---|

| Backend | https://github.com/rfvaldezv/Loyaltybenefit-API | Tag `v0.1.0-wip` |

| Frontend | https://github.com/rfvaldezv/Loyaltybenefit-App | Vacío (solo README) |



\*\*⚠️ Verificar que ambos sean PRIVADOS\*\* (Settings → Danger Zone → visibilidad).



\### Comandos Git esenciales



```powershell

\# Ver estado

cd "C:\\Aplica\\Loyaltybenefit\_API"

git status

git log --oneline -10

git remote -v



\# Hacer commit de cambios

git add .

git commit -m "tipo: descripción del cambio"

git push



\# Ver tags

git tag -l



\# Volver a un tag (si algo se rompe)

git checkout v0.1.0-wip

```



\---



\## ⚙️ 4. CONFIGURACIÓN LOCAL (crítica — NO está en Git)



\### Archivo: `C:\\Aplica\\Loyaltybenefit\_API\\src\\Loyaltybenefit.Api\\appsettings.Development.json`



\*\*Este archivo NO está en Git\*\* (por seguridad). Si pierdes la laptop, tendrás que recrearlo con este contenido:



```json

{

&#x20; "Logging": {

&#x20;   "LogLevel": {

&#x20;     "Default": "Information",

&#x20;     "Microsoft.AspNetCore": "Warning"

&#x20;   }

&#x20; },

&#x20; "ConnectionStrings": {

&#x20;   "Platform": "Server=AIMe-RFVV\\\\SQLEXPRESS2022;Database=LoyaltybenefitAPI;Trusted\_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"

&#x20; },

&#x20; "DatabaseProvisioning": {

&#x20;   "Site4Now": {

&#x20;     "Host": "AIMe-RFVV\\\\SQLEXPRESS2022",

&#x20;     "Port": 1433,

&#x20;     "Username": "sa",

&#x20;     "Password": "DevelopmentOnly\_ChangeMe123!"

&#x20;   }

&#x20; }

}

```



\*\*Notas:\*\*

\- `Server=AIMe-RFVV\\SQLEXPRESS2022` → instancia local de SQL Server Express 2022.

\- `Username/Password` del `Site4Now` son \*\*dummy\*\* (solo para que el seeder no falle en desarrollo).

\- En producción, estas credenciales serán reales y vendrán por variables de entorno.



\---



\## 🗄️ 5. BASE DE DATOS



\### Estado actual



| Item | Valor |

|---|---|

| Motor | SQL Server 2022 Express (instancia `AIMe-RFVV\\SQLEXPRESS2022`) |

| Autenticación | Windows Authentication (Trusted Connection) |

| BD Plataforma | `LoyaltybenefitAPI` |

| Tablas | `Tenants`, `TenantDatabases`, `DatabaseServers`, `DatabaseCredentials`, `Industries`, `\_\_EFMigrationsHistory` |

| Migraciones aplicadas | 4 (`InitialPlatform`, `AddTenantDatabaseStatus`, `AddDatabaseInfrastructure`, `FixTenantConfiguration`) |



\### Verificar que la BD existe



```powershell

sqlcmd -S "AIMe-RFVV\\SQLEXPRESS2022" -E -Q "SELECT name FROM sys.databases WHERE name = 'LoyaltybenefitAPI'"

```



\### Ver tablas



```powershell

sqlcmd -S "AIMe-RFVV\\SQLEXPRESS2022" -E -d "LoyaltybenefitAPI" -Q "SELECT TABLE\_NAME FROM INFORMATION\_SCHEMA.TABLES WHERE TABLE\_TYPE = 'BASE TABLE' ORDER BY TABLE\_NAME"

```



\### Si la BD se perdió (recrear desde cero)



```powershell

$env:Loyaltybenefit\_PLATFORM\_CONNECTION="Server=AIMe-RFVV\\SQLEXPRESS2022;Database=LoyaltybenefitAPI;Trusted\_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"

cd "C:\\Aplica\\Loyaltybenefit\_API"

dotnet ef database update --project src\\Loyaltybenefit.Infrastructure\\Loyaltybenefit.Infrastructure.csproj --startup-project src\\Loyaltybenefit.Api\\Loyaltybenefit.Api.csproj --context PlatformDbContext

```



\---



\## 🚀 6. CÓMO CORRER EL PROYECTO EN 5 MINUTOS (desde cero)



\### Requisitos previos

\- ✅ .NET 10 SDK instalado

\- ✅ SQL Server 2022 Express (instancia `SQLEXPRESS2022`) corriendo

\- ✅ Git instalado

\- ✅ Flutter (para el frontend, aún no necesario)



\### Paso 1 — Clonar repos (si es una máquina nueva)



```powershell

cd "C:\\Aplica"

git clone https://github.com/rfvaldezv/Loyaltybenefit-API.git Loyaltybenefit\_API

git clone https://github.com/rfvaldezv/Loyaltybenefit-App.git Loyaltybenefit\_App

```



\### Paso 2 — Crear el `appsettings.Development.json` (si es máquina nueva)



```powershell

notepad "C:\\Aplica\\Loyaltybenefit\_API\\src\\Loyaltybenefit.Api\\appsettings.Development.json"

```



Pegar el contenido de la sección 4 de este documento.



\### Paso 3 — Restaurar paquetes



```powershell

cd "C:\\Aplica\\Loyaltybenefit\_API"

dotnet restore

```



\### Paso 4 — Aplicar migraciones (si la BD no existe)



```powershell

$env:Loyaltybenefit\_PLATFORM\_CONNECTION="Server=AIMe-RFVV\\SQLEXPRESS2022;Database=LoyaltybenefitAPI;Trusted\_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"

dotnet ef database update --project src\\Loyaltybenefit.Infrastructure\\Loyaltybenefit.Infrastructure.csproj --startup-project src\\Loyaltybenefit.Api\\Loyaltybenefit.Api.csproj --context PlatformDbContext

```



\### Paso 5 — Correr el backend



```powershell

cd "C:\\Aplica\\Loyaltybenefit\_API"

dotnet run --project src\\Loyaltybenefit.Api\\Loyaltybenefit.Api.csproj

```



Esperar a ver:

```

info: Microsoft.Hosting.Lifetime\[14]

&#x20;     Now listening on: http://localhost:5265

info: Microsoft.Hosting.Lifetime\[0]

&#x20;     Application started. Press Ctrl+C to shut down.

info: Microsoft.Hosting.Lifetime\[0]

&#x20;     Hosting environment: Development

```



\### Paso 6 — Verificar que funciona



\- 👉 \*\*http://localhost:5265/\*\* → JSON de bienvenida

\- 👉 \*\*http://localhost:5265/health\*\* → JSON de health

\- 👉 \*\*http://localhost:5265/swagger\*\* → Swagger UI



\### Paso 7 — Detener



`Ctrl + C` en la terminal del `dotnet run`.



\---



\## 🎯 7. COMANDOS ÚTILES (chuleta rápida)



```powershell

\# ═══════════ BACKEND ═══════════



\# Compilar

cd "C:\\Aplica\\Loyaltybenefit\_API"

dotnet build



\# Correr

dotnet run --project src\\Loyaltybenefit.Api\\Loyaltybenefit.Api.csproj



\# Correr el Worker (jobs)

dotnet run --project src\\Loyaltybenefit.Worker\\Loyaltybenefit.Worker.csproj



\# Aplicar migraciones

$env:Loyaltybenefit\_PLATFORM\_CONNECTION="Server=AIMe-RFVV\\SQLEXPRESS2022;Database=LoyaltybenefitAPI;Trusted\_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"

dotnet ef database update --project src\\Loyaltybenefit.Infrastructure\\Loyaltybenefit.Infrastructure.csproj --startup-project src\\Loyaltybenefit.Api\\Loyaltybenefit.Api.csproj --context PlatformDbContext



\# Crear nueva migración

dotnet ef migrations add NOMBRE\_MIGRACION --project src\\Loyaltybenefit.Infrastructure\\Loyaltybenefit.Infrastructure.csproj --startup-project src\\Loyaltybenefit.Api\\Loyaltybenefit.Api.csproj --context PlatformDbContext



\# ═══════════ GIT ═══════════



\# Estado

cd "C:\\Aplica\\Loyaltybenefit\_API"

git status

git log --oneline -10



\# Commit + push

git add .

git commit -m "tipo: descripción"

git push



\# ═══════════ SQL ═══════════



\# Ver BDs

sqlcmd -S "AIMe-RFVV\\SQLEXPRESS2022" -E -Q "SELECT name FROM sys.databases"



\# Ver tablas de la BD plataforma

sqlcmd -S "AIMe-RFVV\\SQLEXPRESS2022" -E -d "LoyaltybenefitAPI" -Q "SELECT TABLE\_NAME FROM INFORMATION\_SCHEMA.TABLES WHERE TABLE\_TYPE = 'BASE TABLE' ORDER BY TABLE\_NAME"



\# ═══════════ FRONTEND (pendiente) ═══════════



\# Crear proyecto Flutter (cuando llegue el momento)

cd "C:\\Aplica\\Loyaltybenefit\_App"

flutter create . --org com.loyaltybenefit --project-name loyalty\_benefit\_app



\# Correr

flutter run -d chrome   # web

flutter run -d windows  # windows desktop

```



\---



\## ⚠️ 8. RIESGOS CONOCIDOS (pendientes por resolver)



| Prioridad | Riesgo | Impacto | Acción |

|---|---|---|---|

| 🔴 Alta | `System.Security.Cryptography.Pkcs` 9.0.4 tiene vulnerabilidad alta | Seguridad | Actualizar a versión parcheada (ticket #1) |

| 🔴 Alta | Endpoints sin autenticación JWT | Seguridad | Implementar auth (semana 2) |

| 🟡 Media | Sin tests automatizados | Calidad | Agregar xUnit + primer test de earn/redeem |

| 🟡 Media | Sin CI/CD (GitHub Actions) | Calidad | Configurar build automático en cada push |

| 🟡 Media | Frontend 0% | Producto incompleto | Arrancar Flutter (semana 2) |

| 🟢 Baja | Sin observabilidad (logs estructurados) | Debugging | Agregar Serilog |

| 🟢 Baja | Sin endpoint de `/health` detallado | Monitoreo | Ya existe `/health` básico |



\---



\## 🎯 9. PRÓXIMOS PASOS (plan inmediato)



\### Sesión 2 — Documentación y primer ticket

1\. Crear `README.md` del backend.

2\. Crear `docs/DIAGNOSTICO.md`.

3\. Crear `docs/ROADMAP.md` (12 semanas).

4\. Crear `docs/MVP.md`.

5\. \*\*Ticket #1:\*\* Actualizar `System.Security.Cryptography.Pkcs` (vulnerabilidad).

6\. \*\*Ticket #2:\*\* Configurar GitHub Actions (`dotnet build` en cada push).

7\. Commit + push.



\### Sesión 3 — Frontend Flutter (arranque)

1\. Crear proyecto Flutter en `Loyaltybenefit\_App`.

2\. Configurar Riverpod.

3\. Pantalla de login.

4\. Conexión al backend (`http://localhost:5265`).

5\. Dashboard vacío con "Hola, \[usuario]".



\### Sesión 4 — Flujo end-to-end

1\. Login real con backend (JWT).

2\. Pantalla de tenants (listar, crear).

3\. Pantalla de loyalty (earn, redeem).

4\. \*\*Primer flujo completo funcional.\*\*



\---



\## 🧠 10. DECISIONES TOMADAS (no perder)



\- ✅ El producto se llama \*\*Loyalty Benefit\*\*.

\- ✅ Stack: \*\*.NET 10 + SQL Server + Flutter\*\*.

\- ✅ Arquitectura: \*\*SaaS multi-tenant con BD por tenant\*\*.

\- ✅ Primer vertical: \*\*hotelería\*\*.

\- ✅ Primer cliente: \*\*Concept Hotel\*\* (Zamora, Michoacán).

\- ✅ Documento `Experience` (encuestas con AI) = \*\*H2 del roadmap\*\*, no MVP.

\- ✅ No más rebrandings. El nombre queda fijo: `Loyaltybenefit\_API` y `Loyaltybenefit\_App`.



\---



\## 🆘 11. SI ALGO SE ROMPE (protocolo de emergencia)



\### Escenario A — "No compila y no sé por qué"

```powershell

cd "C:\\Aplica\\Loyaltybenefit\_API"

git status                     # Ver si hay cambios sin commitear

git stash                      # Guardar cambios locales temporalmente

git pull                       # Traer lo último de GitHub

dotnet clean

dotnet build

```



\### Escenario B — "Perdí el appsettings.Development.json"

→ Recrearlo con la sección 4 de este documento.



\### Escenario C — "Borré la BD sin querer"

→ Recrearla con los comandos de la sección 5.



\### Escenario D — "Todo se rompió, quiero volver al estado de hoy"

```powershell

cd "C:\\Aplica\\Loyaltybenefit\_API"

git checkout v0.1.0-wip

git checkout -b restaurar-desde-hoy

```



\### Escenario E — "Necesito recuperar el EIE original"

→ Está en `C:\\Aplica\\Loyaltybenefit\_Legacy\\api-legacy` con tag `v1.0.0-legacy-eie`.

→ Y también en `C:\\Aplica\\\_backup-20261016-2001\\eie\_api`.



\### Escenario F — "Perdí la laptop entera"

1\. Instalar: Git, .NET 10 SDK, SQL Server 2022 Express, Flutter.

2\. Clonar los 2 repos de GitHub (sección 3).

3\. Recrear `appsettings.Development.json` (sección 4).

4\. Aplicar migraciones (sección 5).

5\. Correr el backend (sección 6).

6\. \*\*Listo en 30 minutos.\*\*



\---



\## 📞 12. CONTACTOS Y RECURSOS



\- \*\*GitHub:\*\* https://github.com/rfvaldezv

\- \*\*Proyectos:\*\* `Loyaltybenefit-API` y `Loyaltybenefit-App`

\- \*\*Última sesión:\*\* 16/Oct/2026

\- \*\*Próxima sesión:\*\* Documentación + frontend Flutter



\---



\## ✅ CHECKLIST DE RETOMA (5 minutos)



Si vuelves al proyecto después de un tiempo, verifica en orden:



\- \[ ] 1. `cd "C:\\Aplica\\Loyaltybenefit\_API"` y `git status` → debe estar limpio o con cambios que reconozcas.

\- \[ ] 2. `git log --oneline -5` → confirmar los últimos commits.

\- \[ ] 3. `Test-Path "C:\\Aplica\\Loyaltybenefit\_API\\src\\Loyaltybenefit.Api\\appsettings.Development.json"` → debe devolver `True`.

\- \[ ] 4. `sqlcmd -S "AIMe-RFVV\\SQLEXPRESS2022" -E -Q "SELECT name FROM sys.databases WHERE name = 'LoyaltybenefitAPI'"` → debe devolver `LoyaltybenefitAPI`.

\- \[ ] 5. `dotnet build` → debe decir "Compilación correcta".

\- \[ ] 6. `dotnet run --project src\\Loyaltybenefit.Api\\Loyaltybenefit.Api.csproj` → debe arrancar en `http://localhost:5265`.

\- \[ ] 7. Abrir `http://localhost:5265/health` → debe devolver `{"status":"healthy"}`.

\- \[ ] 8. Abrir `http://localhost:5265/swagger` → debe abrir Swagger.



\*\*Si los 8 pasos funcionan → el proyecto está 100% operativo y listo para continuar.\*\*



\---



\*\*FIN DEL DOCUMENTO DE CONTINUIDAD\*\*



\*Última actualización: 16/Oct/2026\*

\*Sesión 1 — Rescate y organización\*



---

## 🛑 CHECKPOINT OFICIAL: v0.2.0-alpha (19/Oct/2026)

Si algo se rompe, usa el comando `git checkout v0.2.0-alpha` en ambos repos para volver a este estado exacto.

### ¿Qué funciona en este punto?
1. **Backend:** API en .NET 10 con base de datos SQL Server.
2. **Autenticación:** Endpoint `/api/auth/login` funcional con Token JWT.
3. **Hoteles:** Endpoint `/api/tenants` (GET y POST) protegido con JWT.
4. **Clientes:** Endpoint `/api/customers` (POST) protegido.
5. **Lealtad:** Endpoint `/api/loyalty/earn` protegido (actualmente espera IDs reales de Estancia/Stay).
6. **Frontend:** Flutter conectado. Pantalla de Login, Lista de Hoteles, Registro de Clientes y pantalla de prueba de puntos.

### ¿Qué falta para la siguiente sesión (v0.3.0)?
- Implementar el endpoint y la vista para crear "Estancias" (Stays).
- Hacer que el formulario de clientes cree un "Guest" real en la base de datos (actualmente es una simulación en memoria).
- Conectar los puntos reales al cliente usando la regla de lealtad.

### Para correr el proyecto:
1. Backend: `dotnet run --project src\Loyaltybenefit.Api\Loyaltybenefit.Api.csproj`
2. Frontend: `cd C:\Aplica\Loyaltybenefit_App` y `flutter run -d windows`
3. Usuario de prueba: `admin@loyaltybenefit.com` / `Admin123!`

---


---

## 🛑 CHECKPOINT OFICIAL: v0.3.0-alpha (21/Oct/2026)

Si algo se rompe, usa el comando `git checkout v0.3.0-alpha` en ambos repos para volver a este estado exacto.

### ¿Qué se logró en esta versión?
1. **Seguridad:** Token JWT implementado y validado en todos los endpoints.
2. **Multi-Tenant (Estructura):** El sistema identifica el `TenantId` y se prepara para usar la BD de cada hotel.
3. **Clientes:** Registro de huéspedes/cliente conectado a la API.
4. **Motor de Lealtad:** El sistema calcula puntos automáticamente basados en el consumo (1 punto por cada $100).
5. **Frontend:** Flujo completo desde el Login hasta el cálculo y notificación de puntos.

### ¿Qué está SIMULADO (pendiente para la siguiente sesión)?
- El guardado físico de los puntos en la base de datos del Hotel (actualmente se calculan y se devuelve el saldo, pero no se persiste en el `TenantDbContext`).
- La creación física de la base de datos del "Concept Hotel" (Falta el provisioning real).

### ¿Cómo probar la app?
1. Backend: `dotnet run --project src\Loyaltybenefit.Api\Loyaltybenefit.Api.csproj`
2. Frontend: `cd C:\Aplica\Loyaltybenefit_App` y `flutter run -d windows`
3. Login: `admin@loyaltybenefit.com` / `Admin123!`
4. Flujo: Entrar a Concept Hotel -> Nuevo Cliente -> Registrar Estancia (Monto: 1500) -> Ver mensaje "Puntos otorgados: 15".


---

## 🛑 CHECKPOINT OFICIAL: v1.0.0-beta (Producción en VPS)

### URLs de Producción
- **API:** https://api.loyaltybenefit.com
- **Admin:** https://admin.loyaltybenefit.com
- **App:** https://app.loyaltybenefit.com

### Servidor de Producción
- **Proveedor:** Contabo
- **SQL Server Local:** `VMI1203198\MSSQLSERVER2019`
- **Usuario SA:** `sa` / `Aguador#3985`
- **Carpeta de la API en IIS:** `C:\inetpub\wwwroot\loyaltybenefit_api`

### Configuración de IIS
- La API corre con el **AspNetCoreModuleV2** y los headers de CORS se manejan desde el código C# (no desde `web.config`).
- **IMPORTANTE:** El `web.config` en el VPS **NO debe tener** la sección `<customHeaders>` con `Access-Control-Allow-Origin` (porque causa headers duplicados y bloquea el CORS).

### Credenciales de Acceso
- **Admin:** `admin@loyaltybenefit.com` / `Admin123!`
- **SA SQL Server:** `Aguador#3985`

### Cómo publicar cambios (Flujo de Producción)
1. **Backend:** `dotnet publish -c Release -r win-x64 --self-contained false -o .\publish_sc` y copiar `publish_sc` al VPS en `C:\inetpub\wwwroot\loyaltybenefit_api`. Luego reciclar el Application Pool en IIS.
2. **Admin/App:** `flutter build web --release` y copiar el contenido de `build\web` al VPS en `C:\inetpub\Loyaltybenefit\admin` y `C:\inetpub\Loyaltybenefit\app` respectivamente.

