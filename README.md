# Prueba técnica Abank - API de usuarios

API para gestionar usuarios con login por JWT. Está hecha en .NET 10, con Dapper y PostgreSQL.

- `BaseDeDatos/`: scripts de la tabla, triggers e inserts
- `Backend/`: la solución .NET con los tests

## Levantarlo

Con Docker es un solo comando desde la raíz:

```bash
docker compose up -d --build
```

Levanta Postgres, que la primera vez corre los scripts de `BaseDeDatos/` en orden, y la API en http://localhost:8080. El Swagger está en http://localhost:8080/swagger. Para borrar todo, incluida la base: `docker compose down -v`.

Si quieres correr la API con `dotnet run` (necesitas el SDK de .NET 10), puedes usar solo la base del compose:

```bash
docker compose up -d db
cd Backend
dotnet run --project src/Abank.Usuarios.Api
```

En ese caso la API queda en http://localhost:5000/swagger. Si usas otro Postgres, corre los scripts 01, 02 y 03 y cambia la cadena de conexión en `appsettings.json`.

## Probarlo

Usuarios que crea el script `03_datos.sql`:

| Teléfono | Password |
|---|---|
| 70000000 | Admin12345 |
| 71234567, 72345678, 22501234 | Prueba123 |
| 76543210 | Prueba123 (está inactivo, no deja entrar) |

Haces login en `POST /api/v1/auth/login` con `{ "telefono": "70000000", "password": "Admin12345" }`, copias el `access_token`, y en Swagger le das a Authorize y lo pegas.

También dejé el archivo `Backend/src/Abank.Usuarios.Api/Abank.Usuarios.Api.http` con todos los requests listos.

Endpoints:

| Método | Ruta | Token |
|---|---|---|
| POST | /api/v1/auth/login | no |
| GET | /api/v1/users | no |
| GET | /api/v1/users/{id_user} | sí |
| POST | /api/v1/users | sí |
| PUT | /api/v1/users/{id_user} | sí |
| DELETE | /api/v1/users/{id_user} | sí |

El listado acepta `?page=1&pageSize=50` (máximo 100) y devuelve el total en el header `X-Total-Count`. Hay un `/health` que revisa la conexión a la base. Los errores vienen como ProblemDetails: 400 con los errores por campo, 404, y 409 si el teléfono o el email ya existen.

## Tests

```bash
cd Backend
dotnet test
```

Los unitarios prueban los servicios, las validaciones, el hash y el token. Los de integración levantan un Postgres con Testcontainers, le aplican los scripts de `BaseDeDatos/` y prueban la API completa, incluido el constraint de estado y los triggers. Para esos necesitas Docker corriendo. Los dos también corren en GitHub Actions en cada push.

## Base de datos

Los tipos que dejaban a criterio quedaron así:

- `id`: integer con la secuencia `usuarios_id_seq`.
- `nombres` y `apellidos`: varchar(100).
- `direccion`: varchar(250).
- `email`: varchar(150).
- `telefono`: varchar(20), no numérico, para no perder un `+` o un cero al inicio.
- `fecha_nacimiento`: date.
- Las fechas de auditoría: timestamptz.
- `password`: los 120 que pedían. Ahí se guarda el hash de bcrypt, nunca el texto.

`estado` tiene el constraint para A o I, y por defecto es A. El trigger llena `fecha_creacion` al insertar y `fecha_modificacion` en cada update. Además no deja cambiar `fecha_creacion` después.

El teléfono y el email son únicos, pero solo entre los usuarios activos. Si eliminas a alguien, ese teléfono se puede volver a registrar.

## Algunas decisiones

El login es con teléfono, como en el ejemplo del documento. Respeté los nombres de los JSON de los ejemplos (`fechanacimiento`, `session_active`, `address` en el login), pero el password no lo devuelvo en ninguna respuesta. En los ejemplos viene hasta el hash, y no le veo utilidad para el cliente.

El DELETE no borra la fila: pasa al usuario a estado I. Desde ahí deja de salir en la API y ya no puede hacer login. Para eso me pareció que estaba el campo Estado, y en un banco no se borran clientes así nomás.

El password pide mínimo 8 caracteres, con al menos una letra y un número, así que el `"123"` del ejemplo devuelve 400. El máximo es 72 porque bcrypt ignora lo que pasa de ahí. En el PUT el password es opcional: si no lo mandas, se queda el que tenía.

Para los duplicados no hago un SELECT antes de insertar. Dejo que el índice único falle y lo convierto en 409. Así no se cuelan dos requests al mismo tiempo, y es una consulta menos.

El login tiene un límite de 10 intentos por minuto por IP. Además, cuando el teléfono no existe, igual se compara el password contra un hash cualquiera, para que no se pueda saber por el tiempo de respuesta qué teléfonos están registrados.
