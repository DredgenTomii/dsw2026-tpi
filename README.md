# Trabajo Práctico Integrador — Desarrollo de Software 2026
## API REST desarrollada para el Trabajo Práctico Integrador de la asignatura Desarrollo de Software.
### Integrantes
•	Autino, Matías Daniel — 55802 — 3k3
•	Carabajal, Agustín — 52295 — 3k4
•	Recalde, Tomás Andrés — 56703 — 3k4
•	Valverdi, Agustina Milagros — 53490 — 3k4
________________________________________

### Tecnologías
El proyecto está desarrollado utilizando:
•	C#
•	.NET 10
•	ASP.NET Core Web API
•	Entity Framework Core
•	SQL Server
•	ASP.NET Core Identity
•	JWT Bearer Authentication
•	Swagger / OpenAPI
•	Serilog
________________________________________

### Configuración y ejecución local
Requisitos
Para ejecutar el proyecto localmente se necesita:
•	.NET 10 SDK
•	SQL Server o SQL Server LocalDB
•	Git
•	Un IDE compatible con .NET, como Visual Studio o Visual Studio Code
Se puede comprobar la versión instalada de .NET mediante:
dotnet --version
________________________________________
1. Clonar el repositorio
Clonar el repositorio:
git clone https://github.com/DredgenTomii/dsw2026-tpi.git
Ingresar al proyecto:
cd dsw2026-tpi
Para trabajar con la versión de desarrollo:
git checkout development
________________________________________
2. Restaurar las dependencias
Desde la carpeta raíz del repositorio ejecutar:
dotnet restore
________________________________________
3. Configurar la base de datos
La aplicación requiere una base de datos SQL Server para la persistencia de la información.
La cadena de conexión utilizada por el entorno de desarrollo se encuentra en:
Dsw2026Tpi.Api/appsettings.Development.json
Antes de ejecutar la aplicación, verificar que la cadena de conexión corresponda al servidor SQL disponible en el entorno local.
No es necesario modificar la configuración de JWT, Identity u otros servicios para una ejecución local normal, salvo que se quiera utilizar una configuración diferente a la proporcionada por el proyecto.
________________________________________
4. Aplicar las migraciones
Una vez configurada la conexión a la base de datos, ejecutar las migraciones de Entity Framework Core:
dotnet ef database update --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api
Si el comando dotnet ef no está instalado:
dotnet tool install --global dotnet-ef
________________________________________
5. Ejecutar la aplicación
Desde la carpeta raíz:
dotnet run --project Dsw2026Tpi.Api
También es posible ejecutar el proyecto desde Visual Studio seleccionando Dsw2026Tpi.Api como proyecto de inicio.
La consola mostrará las direcciones en las que la aplicación está escuchando.
________________________________________

### Swagger / OpenAPI
Durante el entorno de desarrollo, la aplicación expone la documentación interactiva mediante Swagger.
Una vez iniciada la API, acceder a:
https://localhost:<puerto>/swagger
Swagger permite:
•	Consultar todos los endpoints disponibles.
•	Ver los parámetros de cada operación.
•	Consultar los modelos de solicitud y respuesta.
•	Ejecutar solicitudes directamente contra la API.
•	Probar endpoints que requieren autenticación.
Swagger es la referencia principal para consultar la definición actual de los endpoints y sus modelos.
________________________________________

### Autenticación
La API utiliza autenticación mediante JWT Bearer.
El flujo general para probar los endpoints protegidos es:
1.	Registrar un usuario administrador mediante el endpoint correspondiente.
2.	Iniciar sesión con las credenciales creadas.
3.	Obtener el token JWT.
4.	Utilizar el token para acceder a los endpoints que requieren autenticación.
El token debe enviarse en el encabezado HTTP:
Authorization: Bearer <TOKEN>
En Swagger puede utilizarse el botón Authorize para ingresar el token y realizar las pruebas de los endpoints protegidos.
________________________________________

### Endpoints
Endpoints de la API utilizan principalmente el prefijo:
/api
Autenticación
Método	Endpoint	Descripción	Autenticación
POST	/api/auth/admin/register	Registra un usuario administrador para realizar pruebas.	No
POST	/api/auth/admin/login	Autentica un usuario administrador y devuelve un token.	No
POST	/api/auth/patient/login	Autentica un paciente y devuelve un token.	No
El endpoint de registro de administradores se encuentra disponible para facilitar las pruebas del sistema.
________________________________________

### Médicos
Endpoints de médicos requieren autenticación con permisos de administrador.
Método	Endpoint	Descripción
GET	/api/doctors	Obtiene médicos de forma paginada y permite filtrarlos.
POST	/api/doctors	Registra un nuevo médico.
PUT	/api/doctors/{id}	Actualiza la información de un médico existente.
DELETE	/api/doctors/{id}	Elimina un médico.
GET	/api/doctors/{id}/availabilities	Consulta las disponibilidades de un médico.
Los parámetros de paginación, filtros, cuerpos de las solicitudes y formatos de respuesta pueden consultarse directamente en Swagger.
________________________________________

### Health Check
Endpoint para comprobar que la aplicación se encuentra disponible:
GET /health-check
Este endpoint no utiliza el prefijo /api.
________________________________________

### Ejemplo de flujo de prueba
Una prueba básica de la aplicación puede realizarse siguiendo estos pasos:
1. Ejecutar la API
dotnet run --project Dsw2026Tpi.Api
2. Abrir Swagger
Ingresar a:
https://localhost:<puerto>/swagger
3. Registrar un administrador
Ejecutar:
POST /api/auth/admin/register
utilizando el modelo de solicitud mostrado por Swagger.
4. Iniciar sesión
Ejecutar:
POST /api/auth/admin/login
Copiar el token JWT obtenido.
5. Autorizar Swagger
Seleccionar Authorize e ingresar el token utilizando el esquema:
Bearer <TOKEN>
6. Probar los endpoints protegidos
Una vez autenticado, pueden probarse las operaciones disponibles para los recursos protegidos, como la gestión de médicos y la consulta de disponibilidades.
________________________________________

### Estructura del proyecto

Dsw2026Tpi
│
├── Dsw2026Tpi.Api
│   ├── Controllers
│   ├── Configurations
│   ├── Middlewares
│   └── Program.cs
│
├── Dsw2026Tpi.Application
│   ├── Dtos
│   ├── Interfaces
│   └── Services
│
├── Dsw2026Tpi.Domain
│   └── Entidades y reglas de dominio
│
└── Dsw2026Tpi.Data
    ├── Persistencia
    ├── Entity Framework Core
    └── Migraciones
