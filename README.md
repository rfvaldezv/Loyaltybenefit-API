# Loyaltybenefit_API

Backend .NET migrado de la copia fuente del proyecto. Se conservaron las capas y las migraciones; los nombres de solución, carpetas y proyectos se normalizaron a Loyaltybenefit.

## Requisitos
- .NET SDK 10 para compilar localmente, o Docker con soporte para imágenes .NET 10.
- SQL Server accesible desde el proceso de API/Worker.

## Compilar y probar
```bash
dotnet restore Loyaltybenefit_API.slnx
dotnet build Loyaltybenefit_API.slnx -c Release
dotnet test Loyaltybenefit_API.slnx -c Release
```

## Docker
1. Copia `.env.example` a `.env` y completa valores reales.
2. No publiques `.env` ni credenciales en Git.
3. Ejecuta `docker compose up -d --build`.
4. El puerto de API queda enlazado a `127.0.0.1:8080`; publica por Nginx/HTTPS, no expongas el servicio directamente sin revisar autenticación y autorización.

## Antes de producción
- Revisar autenticación/autorización y la validación del tenant en cada petición.
- Revisar idempotencia, concurrencia y reglas de canje/acumulación.
- Validar y probar migraciones en una base de datos de ensayo y respaldar antes de migrar producción.
- Revisar configuración de aprovisionamiento SQL, permisos mínimos y cifrado de conexión.
- La compilación no se ha verificado en este entorno porque no se dispone del SDK de .NET.
