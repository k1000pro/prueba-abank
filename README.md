# Prueba técnica Abank - API de usuarios

API REST para gestionar usuarios y el acceso con JWT. Hecha en .NET 10 con Dapper sobre PostgreSQL.

```
BaseDeDatos/   scripts de creación de objetos e inserts
Backend/       solución .NET (código y tests)
```

## Cómo levantarlo

### Con Docker (lo más rápido)

Solo hace falta Docker. Desde la raíz del repo:

```bash
docker compose up -d --build
```

Esto levanta:

- **Postgres 17** en el puerto 5432. La primera vez corre en orden los scripts de `BaseDeDatos/` (tabla, triggers y datos de prueba).
- **La API** en http://localhost:8080. El Swagger queda en http://localhost:8080/swagger.

Para borrar todo, base de datos incluida: `docker compose down -v`

### Sin Docker para la API

Necesitas el SDK de .NET 10 y un Postgres con los scripts aplicados. Lo más fácil es usar solo la base del compose:

```bash
docker compose up -d db
cd Backend
dotnet run --project src/Abank.Usuarios.Api
```

La API queda en http://localhost:5000/swagger. La cadena de conexión está en `appsettings.json` y la llave del JWT para desarrollo en `appsettings.Development.json`.

Si prefieres usar un Postgres propio, corre los tres scripts de `BaseDeDatos/` en orden (01, 02, 03) y cambia `ConnectionStrings:Postgres`.

## Usuarios de prueba

Los crea `BaseDeDatos/03_datos.sql`:

| Teléfono | Password | Estado |
|---|---|---|
| 70000000 | Admin12345 | A |
| 71234567 | Prueba123 | A |
| 72345678 | Prueba123 | A |
| 22501234 | Prueba123 | A |
| 76543210 | Prueba123 | I (no puede hacer login) |

## Cómo probarlo

1. Hacer login en `POST /api/v1/auth/login`:

   ```json
   { "telefono": "70000000", "password": "Admin12345" }
   ```

2. Copiar el `access_token` de la respuesta.
3. En Swagger, botón **Authorize** y pegar el token (sin la palabra Bearer).

También está `Backend/src/Abank.Usuarios.Api/Abank.Usuarios.Api.http` con todos los requests, para usarlo desde Visual Studio, Rider o VS Code (extensión REST Client).

Con curl:

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"telefono":"70000000","password":"Admin12345"}' | jq -r .access_token)

curl http://localhost:8080/api/v1/users/1 -H "Authorization: Bearer $TOKEN"
```

## Endpoints

| Método | Ruta | Token | Respuesta |
|---|---|---|---|
| POST | /api/v1/auth/login | no | 200 con el usuario y el token |
| GET | /api/v1/users | no | 200 con la lista de usuarios activos |
| GET | /api/v1/users/{id_user} | sí | 200, o 404 si no existe |
| POST | /api/v1/users | sí | 201 con el usuario creado |
| PUT | /api/v1/users/{id_user} | sí | 200 con el usuario actualizado |
| DELETE | /api/v1/users/{id_user} | sí | 200 con el usuario eliminado |

Otros:

- `GET /api/v1/users` acepta `?page=1&pageSize=50` (máximo 100). La respuesta sigue siendo un arreglo, como en el enunciado, y el total viene en el header `X-Total-Count`.
- Los errores salen en formato ProblemDetails: 400 de validación (con el detalle por campo), 401, 404, 409 si el teléfono o el email ya existen y 429 si hay demasiados intentos de login.
- `GET /health` revisa que la API tenga conexión con la base.

Body para crear (en PUT es igual, pero el `password` es opcional: si no se manda, se queda el que tenía):

```json
{
  "nombres": "test",
  "apellidos": "test",
  "fechanacimiento": "1996-05-10",
  "telefono": "73212656",
  "email": "test@gmail.com",
  "password": "Prueba123",
  "direccion": "direccion"
}
```

## Tests

```bash
cd Backend
dotnet test
```

- **Abank.Usuarios.UnitTests**: servicios, validaciones, hash de passwords y generación del token. Los repositorios se mockean con NSubstitute.
- **Abank.Usuarios.IntegrationTests**: levantan un Postgres real con Testcontainers, le aplican los mismos scripts de `BaseDeDatos/` y prueban la API de punta a punta. También revisan el constraint de estado y los triggers. **Necesitan Docker corriendo.**

Si solo quieres correr los unitarios: `dotnet test tests/Abank.Usuarios.UnitTests`

Los tests también corren en GitHub Actions en cada push (`.github/workflows/ci.yml`).

## Estructura del backend

```
Backend/
  src/
    Abank.Usuarios.Domain           entidad Usuario
    Abank.Usuarios.Application      servicios, DTOs, validaciones (FluentValidation) e interfaces
    Abank.Usuarios.Infrastructure   repositorio con Dapper, BCrypt, JWT
    Abank.Usuarios.Api              controllers, manejo de errores, Swagger, configuración
  tests/
    Abank.Usuarios.UnitTests
    Abank.Usuarios.IntegrationTests
```

Application solo conoce interfaces (`IUsuarioRepository`, `IPasswordHasher`, `ITokenService`) y las implementaciones están en Infrastructure. Así los servicios se prueban sin base de datos, y cambiar algo como el hasher no afecta lo demás.

## Base de datos

Los tipos y largos que el enunciado dejaba a criterio:

| Campo | Tipo | Por qué |
|---|---|---|
| id | integer + secuencia `usuarios_id_seq` | se pidió secuencia explícita, integer alcanza de sobra |
| nombres / apellidos | varchar(100) | |
| fecha_nacimiento | date | solo interesa la fecha |
| direccion | varchar(250) | |
| password | varchar(120) | guarda el hash de bcrypt (60 caracteres), nunca el texto plano |
| telefono | varchar(20) | varchar y no número para no perder ceros ni el `+` |
| email | varchar(150) | |
| estado | varchar(1), default 'A' | constraint `ck_usuarios_estado` que solo acepta A o I |
| fecha_creacion / fecha_modificacion | timestamptz | con zona horaria para no tener problemas entre servidores |

- El trigger `tr_usuarios_fechas` llena `fecha_creacion` en el insert y `fecha_modificacion` en cada update. Además impide que se modifique `fecha_creacion` aunque alguien lo intente en un UPDATE.
- Hay índices únicos sobre `telefono` y `lower(email)`, solo para usuarios activos. Si un usuario se elimina, su teléfono se puede volver a registrar.

## Decisiones que tomé

- **El login es con teléfono**, como en el ejemplo del enunciado.
- **El password no se devuelve en ninguna respuesta.** Los ejemplos del enunciado lo traen (incluso el hash en el login), pero exponer el hash no tiene ninguna utilidad para el cliente y es un riesgo. El resto de los campos respeta los nombres de los ejemplos (`fechanacimiento`, `session_active`, `address` en el login, `token_type: "bearer"`). Agregué `expires_in` a la respuesta del login.
- **El DELETE es lógico**: el usuario pasa a estado `I`, deja de salir en la API y ya no puede hacer login. En un banco no se suelen borrar registros de clientes, y para eso tiene sentido el campo Estado.
- **Validaciones**: el teléfono acepta de 8 a 15 dígitos, con `+` opcional. El password necesita mínimo 8 caracteres, con al menos una letra y un número (por eso el `"123"` del ejemplo devuelve 400). El máximo es 72 porque bcrypt ignora lo que pasa de 72 bytes. La fecha de nacimiento tiene que ser anterior a hoy.
- **Duplicados**: no se consulta antes de insertar si el teléfono existe, sino que lo controla el índice único y se traduce el error a un 409. Así se evita que dos requests simultáneos pasen la validación al mismo tiempo, y se ahorra una consulta.
- **El listado** trae la página y el total en un solo viaje a la base, con `QueryMultiple`.
- **Login**: tiene un límite de 10 intentos por minuto por IP. Si el teléfono no existe, igual se compara el password contra un hash de relleno, para que el tiempo de respuesta no revele qué teléfonos están registrados.
- Se usa un solo `NpgsqlDataSource` para toda la app, que maneja el pool de conexiones.
