# CivicFix

Panel web de gestión de reportes ciudadanos, construido con ASP.NET Core MVC, C#, Razor, JavaScript, ASP.NET Core Identity y Entity Framework Core.

## Ejecutar

Requiere el SDK .NET 10.

```powershell
dotnet restore
dotnet run
```

En desarrollo usa SQLite en `Data/civicfix.db`, por lo que las cuentas y los reportes se conservan al reiniciar. La aplicación crea 12 reportes de muestra cuando la base está vacía. Abre la URL local que muestra `dotnet run`. Al entrar a la raíz, las personas sin sesión van al inicio de sesión; después, el sistema redirige a cada cuenta según su rol.

### Roles y acceso

- Las personas se registran desde **Crear cuenta** y reciben únicamente el rol `Citizen`. Pueden enviar reportes con dirección, ubicación en el mapa y fotografía; no pueden entrar al panel ni cambiar el estado de reportes.
- Ciudadanos y administradores usan el mismo formulario de **Iniciar sesión** con su correo y contraseña. Al autenticarse, los ciudadanos llegan al formulario de reportes y los administradores al panel.
- El panel, la asignación, la resolución y la exportación están restringidos al rol `Admin`.
- No hay contraseñas de administrador predeterminadas. Para provisionar la primera cuenta admin, guarda credenciales localmente con User Secrets y después arranca la aplicación:

```powershell
dotnet user-secrets init --project .\CivicFix.csproj
dotnet user-secrets set "CivicFixAdmin:Email" "admin@civicfix.local" --project .\CivicFix.csproj
dotnet user-secrets set "CivicFixAdmin:Password" "UnaClave-Fuerte-123!" --project .\CivicFix.csproj
dotnet run
```

La cuenta se crea con rol `Admin` al iniciar. Cambia el ejemplo de contraseña por una propia. Las cuentas ciudadanas nunca pueden autoasignarse permisos de administrador.
- Para dar acceso Admin a más personas, primero deben crear su cuenta desde **Crear cuenta**. Después, inicia sesión con una cuenta Admin, abre **Administradores** en el menú lateral, busca el correo y selecciona **Dar acceso Admin**. La persona debe cerrar sesión y volver a entrar para que se actualicen sus permisos.

## Usar SQL Server

El proveedor `Microsoft.EntityFrameworkCore.SqlServer` y la cadena de conexión están configurados en `appsettings.json`. Para usar SQL Server, cambia `Database:Provider` a `SqlServer` en `appsettings.Development.json` y configura `ConnectionStrings:CivicFix` con tu instancia. La cadena incluida apunta a SQL Server LocalDB (`(localdb)\MSSQLLocalDB`), que debe estar instalado.

El esquema inicial se crea con `EnsureCreated`. Antes de desplegar, configura una base de datos productiva, secretos fuera de los archivos de configuración y reemplaza `EnsureCreated` por migraciones de EF Core.

## Funciones incluidas

- Panel con métricas, filtros por estado y categoría, búsqueda y exportación CSV.
- Asignación de reportes pendientes y resolución de reportes en proceso.
- Formulario ciudadano con categoría, prioridad, selección de punto en mapa OpenStreetMap, geolocalización y foto JPG, PNG o WEBP de hasta 5 MB.
- Diseño responsive basado en el prototipo visual adjunto.

El mapa usa Leaflet y mosaicos de OpenStreetMap; requiere conexión a internet. Si el mapa no está disponible, el formulario conserva la captura manual de dirección.