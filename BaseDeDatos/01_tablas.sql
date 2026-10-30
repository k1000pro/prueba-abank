-- Tabla de usuarios
-- Los scripts se pueden correr más de una vez sin romper nada.

CREATE SEQUENCE IF NOT EXISTS usuarios_id_seq
    AS INTEGER
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;

CREATE TABLE IF NOT EXISTS usuarios (
    id                  INTEGER      NOT NULL DEFAULT nextval('usuarios_id_seq'),
    nombres             VARCHAR(100) NOT NULL,
    apellidos           VARCHAR(100) NOT NULL,
    fecha_nacimiento    DATE         NOT NULL,
    direccion           VARCHAR(250) NOT NULL,
    password            VARCHAR(120) NOT NULL,
    telefono            VARCHAR(20)  NOT NULL,
    email               VARCHAR(150) NOT NULL,
    estado              VARCHAR(1)   NOT NULL DEFAULT 'A',
    fecha_creacion      TIMESTAMPTZ  NOT NULL DEFAULT now(),
    fecha_modificacion  TIMESTAMPTZ  NULL,

    CONSTRAINT pk_usuarios PRIMARY KEY (id),
    CONSTRAINT ck_usuarios_estado CHECK (estado IN ('A', 'I'))
);

ALTER SEQUENCE usuarios_id_seq OWNED BY usuarios.id;

-- telefono se usa para el login y el email tampoco se debe repetir.
-- Solo aplica a usuarios activos: si alguien se da de baja, su teléfono
-- puede volver a registrarse.
CREATE UNIQUE INDEX IF NOT EXISTS ux_usuarios_telefono ON usuarios (telefono) WHERE estado = 'A';
CREATE UNIQUE INDEX IF NOT EXISTS ux_usuarios_email ON usuarios (lower(email)) WHERE estado = 'A';

-- el listado siempre filtra por estado
CREATE INDEX IF NOT EXISTS ix_usuarios_estado ON usuarios (estado, id);
