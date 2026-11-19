# Trabajo Práctico Integrador — Desarrollo de Software 2026

## API REST desarrollada para el Trabajo Práctico Integrador de la asignatura Desarrollo de Software.



### Integrantes

* Autino, Matías Daniel — 55802 — 3k3

* Cesar Hermida Cordaro — 52437 — 3k3

* Ricardo Nahuel Pumara — 60887 — 3k3

* Valverdi, Agustina Milagros — 53490 — 3k4



---



### Tecnologías

El proyecto está desarrollado utilizando:

* C#

* .NET 10

* ASP.NET Core Web API

* Entity Framework Core

* SQL Server

* ASP.NET Core Identity

* JWT Bearer Authentication

* Swagger / OpenAPI

* Serilog



---



### Configuración y ejecución local



#### Requisitos

Para ejecutar el proyecto localmente se necesita:

* .NET 10 SDK

* SQL Server o SQL Server LocalDB

* Git

* Un IDE compatible con .NET, como Visual Studio o Visual Studio Code



Se puede comprobar la versión instalada de .NET mediante:

```bash

dotnet --version

```



#### 1. Clonar el repositorio

Clonar el repositorio:

```bash

git clone https://github.com/DredgenTomii/dsw2026-tpi.git

```

Ingresar al proyecto:

```bash

cd dsw2026-tpi

```

Para trabajar con la versión de desarrollo:

```bash

git checkout development

```



#### 2. Restaurar las dependencias

Desde la carpeta raíz del repositorio ejecutar:

```bash

dotnet restore

```



#### 3. Configurar la base de datos

La aplicación requiere una base de datos SQL Server para la persistencia de la información.

La cadena de conexión utilizada por el entorno de desarrollo se encuentra en:

`Dsw2026Tpi.Api/appsettings.Development.json`



Antes de ejecutar la aplicación, verificar que la cadena de conexión corresponda al servidor SQL disponible en el entorno local.

No es necesario modificar la configuración de JWT, Identity u otros servicios para una ejecución local normal, salvo que se quiera utilizar una configuración diferente a la proporcionada por el proyecto.



#### 4. Aplicar las migraciones

Una vez configurada la conexión a la base de datos, ejecutar las migraciones de Entity Framework Core:

```bash

dotnet ef database update --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api

```

Si el comando `dotnet ef` no está instalado:

```bash

dotnet tool install --global dotnet-ef

```



#### 5. Ejecutar la aplicación

Desde la carpeta raíz:

```bash

dotnet run --project Dsw2026Tpi.Api

```

También es posible ejecutar el proyecto desde Visual Studio seleccionando `Dsw2026Tpi.Api` como proyecto de inicio.

La consola mostrará las direcciones en las que la aplicación está escuchando.



---



### Swagger / OpenAPI

Durante el entorno de desarrollo, la aplicación expone la documentación interactiva mediante Swagger.

Una vez iniciada la API, acceder a:

`https://localhost:<puerto>/swagger`



Swagger permite:

* Consultar todos los endpoints disponibles.

* Ver los parámetros de cada operación.

* Consultar los modelos de solicitud y respuesta.

* Ejecutar solicitudes directamente contra la API.

* Probar endpoints que requieren autenticación.



Swagger es la referencia principal para consultar la definición actual de los endpoints y sus modelos.



---



### Autenticación y Credenciales iniciales

La API utiliza autenticación mediante JWT Bearer.



Al iniciar por primera vez se crea automáticamente el administrador inicial:

* **Email:** `admin@system.com`

* **Contraseña:** `Admin12345!`



*(Configurable en la sección `AdminSeed` de `appsettings.json` / `appsettings.Development.json`)*.



El flujo general para probar los endpoints protegidos es:

1. Iniciar sesión con las credenciales del administrador (o como paciente ingresando email y DNI).

2. Obtener el token JWT devuelto por la API.

3. Utilizar el token para acceder a los endpoints que requieren autenticación enviándolo en el encabezado HTTP:

   `Authorization: Bearer <TOKEN>`



En Swagger puede utilizarse el botón **Authorize** para ingresar el token y realizar las pruebas de los endpoints protegidos.



---



### Endpoints

Los endpoints de la API utilizan principalmente el prefijo `/api`:



| Método | Endpoint | Rol | Descripción |

|---|---|---|---|

| POST | `/api/auth/admin/login` | público | Login admin (`email`, `password`) |

| POST | `/api/auth/patient/login` | público | Login paciente (`email`, `dni`). Lo crea si no existe |

| GET | `/api/specialties?name=` | ambos | Lista especialidades activas |

| POST / PUT / DELETE | `/api/specialties/{id}` | admin | Alta, edición y baja lógica |

| GET | `/api/doctors?name=&specialtyId=` | ambos | Lista médicos activos |

| POST / PUT / DELETE | `/api/doctors/{id}` | admin | Alta, edición y baja lógica |

| GET | `/api/doctors/{id}/availabilities` | ambos | Horario mensual por día |

| GET | `/api/doctors/{id}/slots?from=&to=` | ambos | Turnos de 30 min libres |

| POST / PUT | `/api/availabilities` | admin | Carga o reemplaza la disponibilidad del mes |

| POST | `/api/appointments` | paciente | Reserva un turno |

| GET | `/api/appointments/patient?dni=` | paciente | Turnos activos del paciente |

| DELETE | `/api/appointments/{id}` | paciente | Cancela un turno `BOOKED` |

| GET | `/api/appointments?date=` | admin | Turnos del día |

| GET | `/api/appointments/search?specialtyId=&doctorId=&dni=&date=` | admin | Búsqueda combinada |



#### Health Check

Endpoint para comprobar que la aplicación se encuentra disponible (no utiliza el prefijo `/api`):

* `GET /health-check`



---



### Ejemplo de flujo de prueba

Una prueba básica de la aplicación puede realizarse siguiendo estos pasos:



1. **Ejecutar la API:**

   ```bash

   dotnet run --project Dsw2026Tpi.Api

   ```

2. **Abrir Swagger:**

   Ingresar a `https://localhost:<puerto>/swagger`

3. **Iniciar sesión como Administrador:**

   Ejecutar `POST /api/auth/admin/login` con las credenciales iniciales (`admin@system.com` / `Admin12345!`) y copiar el token JWT obtenido.

4. **Autorizar Swagger:**

   Seleccionar **Authorize** e ingresar el token utilizando el esquema `Bearer <TOKEN>`.

5. **Probar los endpoints protegidos:**

   Una vez autenticado, pueden probarse las operaciones disponibles para los recursos protegidos, como la gestión de especialidades, médicos, disponibilidades y turnos.



---



### Estructura del proyecto



```text

Dsw2026Tpi

├── Dsw2026Tpi.Api

│   ├── Controllers

│   ├── Configurations

│   ├── Middlewares

│   └── Program.cs

├── Dsw2026Tpi.Application

│   ├── Dtos

│   ├── Interfaces

│   └── Services

├── Dsw2026Tpi.Domain

│   └── Entidades y reglas de dominio

└── Dsw2026Tpi.Data

    ├── Persistencia

    ├── Entity Framework Core

    └── Migraciones
